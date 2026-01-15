using Daab.Modules.Scientists.Models;
using Daab.SharedKernel;

namespace Daab.Modules.Scientists.Persistence.Repositories;

public interface IScientistsRepository : IRepository<Scientist> { }

public class ScientistsRepository(ScientistsContext context)
    : RepositoryBase<Scientist>(context),
        IScientistsRepository { }

