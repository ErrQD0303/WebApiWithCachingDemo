using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiWithCachingDemo.Repository
{
    public class UnitOfWork(ICurrentTimeRepository currentTimeRepository) : IUnitOfWork
    {
        public ICurrentTimeRepository CurrentTimeRepository { get; init; } = currentTimeRepository;
    }
}