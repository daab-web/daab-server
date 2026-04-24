.PHONY: run watch add-activities-migration add-scientists-migration add-auth-migration

define add-migration
	@[ -n "$(name)" ] || { echo "Usage: make $@ name=MigrationName"; exit 1; }
	dotnet ef migrations add $(name) \
		-s src/Daab.Web \
		-p src/Modules/$(1) \
		-c $(2) \
		-o Persistence/Migrations
endef

run:
	dotnet run --project ./src/Daab.Web

watch:
	dotnet watch --project ./src/Daab.Web --no-hot-reload

add-activities-migration:
	$(call add-migration,Activities,ActivitiesDbContext)

add-scientists-migration:
	$(call add-migration,Scientists,ScientistsDbContext)

add-auth-migration:
	$(call add-migration,Auth,AuthDbContext)
