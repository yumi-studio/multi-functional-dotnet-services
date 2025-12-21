using System;
using YumiStudio.Domain.Entities;

namespace YumiStudio.Domain.Interfaces;

public interface ITokenRepository : IRepository<Token, Guid>
{

}
