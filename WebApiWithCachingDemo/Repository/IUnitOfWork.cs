using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiWithCachingDemo.Repository
{
    public interface IUnitOfWork
    {
        ICurrentTimeRepository CurrentTimeRepository { get; init; }
    }
}