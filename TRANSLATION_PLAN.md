# Daab — Agentic Translation Module

Spec for building AI translation of news articles and scientist pages.
Both content bodies are Lexical `SerializedEditorState` JSON.

## Goal

Translate articles and scientist pages so the system **learns from approved
example translations and keeps translating in the same style** — without
fine-tuning a model. Achieved via in-context learning backed by a translation
memory (RAG). Every human-approved translation flows back into the memory and
immediately improves future output. That write-back loop is the whole point:
consistent, ever-improving style for the price of inference, no training.

## Non-negotiable rules

1. **Never send the Lexical JSON blob to the model to translate in place.** It
   will eventually corrupt the node tree, drop a `format` bitmask, or reflow
   structure. Instead: extract text → translate → splice back into a clone of
   the original tree. Structure is preserved by construction.

2. **Translate at the block level, not per node.** In Lexical, "the **quick**
   brown fox" is three text nodes. Translating nodes in isolation destroys
   sentence context — fatal for AZ where word order shifts. Serialize each
   block's inline children into ONE tagged string: bold run → `<0>…</0>`, link →
   `<1>anchor</1>` with the url in a side-map keyed by `1`, linebreak → `<br2/>`.

3. **Glossary forces terms; examples only suggest style.** Names, institutions,
   academic-area labels from `ReferenceData` go in as "always render X as Y."
   Separate layer, stronger than few-shot examples.

## Build order (each step has a gate — do not skip)

### Step 1 — Lexical walker (FIRST, in isolation, test-driven)
Extract (tree → tagged block strings + node-path map) and reinsert (tagged
strings → cloned tree). This is the load-bearing piece.

- Use `JsonNode` (System.Text.Json). Do NOT strongly type Lexical nodes — the
  node types are open-ended.
- Extract at block level (paragraph / heading / list-item). Inline formatting →
  tags. Links: anchor stays inline as `<n>…</n>`, url stashed in side-map.
- Keep a map from each block to its node path for reinsertion.
- Reinsert: parse tagged translation back into text nodes — tagged spans get the
  format bitmask / link url from the side-map, untagged spans → format-0 text
  nodes. Splice into a clone by node path.

**GATE:** round-trip real articles (nested links, lists, headings, mixed inline
formatting) through extract→reinsert with a NO-OP translation and assert the
tree comes back byte-identical. Nothing AI-related until this passes.

### Step 2 — Translation Memory schema + repository
Table columns: source hash, source text, target text, `content_type`
(scientist/article), `block_type` (heading/paragraph/list-item), embedding
(`float[]` / pgvector later).

Tiered lookup per block:
1. **Exact hash match** → reuse target verbatim, NO model call. Doubles as an
   edit cache: re-saving an article only re-translates changed paragraphs.
2. **Near/fuzzy match** (normalized edit distance > ~0.8) → strongest few-shot.
3. **Semantic match** (embedding cosine) → everything else.

Filter by `content_type` + `block_type` BEFORE ranking. Inject tiers 2–3 as
few-shot `source → target` pairs (embed on source side, inject the pair). Apply
MMR-style diversity when selecting K (3–8); don't take K near-duplicates.

**GATE:** seed rows, confirm exact match short-circuits and cosine returns
sensible neighbors on real blocks.

### Step 3 — Glossary projection from ReferenceData
Read-only query → "always render X as Y" lines. Standalone, cheap.

### Step 4 — Translation service (wire together)
Extract → **per-document** retrieval (pool + dedup, one shared example set for
the batch — simpler/cheaper/better coherence than per-block; split to per-block
only if long mixed-subject docs go inconsistent) → assemble prompt → ONE Gemini
call per document returning a JSON array (index → translated string).

Prompt order:
- **Cached prefix** (Gemini context caching): role, style guide, glossary.
  Identical every call — main cost lever.
- Few-shot examples: retrieved pairs, tags present in both source and target.
- Task: untranslated blocks as a numbered list.

Reuse the existing Gemini `HttpClient` pattern from the floor-plan feature.

### Step 5 — Validation + reinsert
Per block: tag multiset in output == input (same ids, balanced, none invented).
Hard checkable invariant. On failure, retry that block with the error fed back.
Check no empty blocks. Then reinsert via Step 1 code. Round-trip result through
the Lexical schema to confirm valid JSON before returning a draft.

### Step 6 — Review → approve → write-back
Approval embeds the block pairs and inserts them into the TM (with embeddings).
Closes the flywheel. Build last — but it's what makes this worth building.

## Tech decisions (settled)

- New `Daab.Modules.Translation` module owning `ITranslationService`, the TM
  table, and the glossary projection.
- **No fine-tuning.** In-context learning only.
- **Gemini via `HttpClient`** (already in place) for translation + embeddings.
- **No vector DB yet.** In-memory cosine over loaded vectors at current scale
  (~80 scientists). Graduate to **pgvector** when corpus outgrows memory — moves
  cosine into Postgres, zero similarity code.
- **No ML.NET.** Embeddings = HTTP call. Similarity = 8-line dot product.
  Optional `System.Numerics.Tensors.TensorPrimitives.CosineSimilarity` later if
  vector count ever grows enough to matter. Fuzzy tier: hand-written normalized
  Levenshtein or a tiny package like Fastenshtein.
- **Gemini context caching** on the static prompt prefix — main cost lever.
- Embed once at approval time, store on the TM row. Never re-embed at query time
  except the incoming block.
- AZ is lower-resource: verify the embedder returns sensible neighbors on real
  blocks before trusting the semantic tier.

## Cold start
TM empty on day one → retrieval returns nothing → every block falls to a floor of
glossary + style guide + a small hand-picked static example set per content type.
Retrieval only ever adds on top. Approvals fill the TM and tiers 1–3 start firing.

## Field handling
- Titles: likely plain strings (not Lexical) → single untagged segment through
  the same path. **CONFIRM before starting** — changes the field-level path.
- Names / slugs / institution labels: do NOT translate. Pull canonical
  target-language value from `ReferenceData`, force via glossary.

## Feed Claude Code up front
- A **real sample** of your Lexical JSON: one article + one scientist bio, with
  links, lists, and mixed formatting present. The walker's correctness depends
  on your actual node structure.
- Your **ReferenceData shape** for the glossary query.

Start the session at Step 1 with the sample JSON. Hold the round-trip gate before
moving on.
