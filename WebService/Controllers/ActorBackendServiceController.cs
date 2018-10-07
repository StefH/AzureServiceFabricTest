using ActorBackendService.Interfaces;
using ActorBackendServiceNetCore.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Query;
using System;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebService.Controllers
{
    [Route("api/[controller]")]
    public class ActorBackendServiceController : Controller
    {
        private readonly FabricClient _fabricClient;
        private readonly ConfigSettings _configSettings;
        private readonly StatelessServiceContext _serviceContext;

        public ActorBackendServiceController(StatelessServiceContext serviceContext, ConfigSettings settings, FabricClient fabricClient)
        {
            _serviceContext = serviceContext;
            _configSettings = settings;
            _fabricClient = fabricClient;
        }

        // GET: api/actorbackendservice
        [HttpGet]
        public async Task<IActionResult> GetAsync(string actorType)
        {
            long count = -1;
            switch (actorType)
            {
                case "netcore":
                    count = await GetCountAsync(_configSettings.ActorBackendServiceNetCoreName);
                    break;

                default:
                    count = await GetCountAsync(_configSettings.ActorBackendServiceName);
                    break;
            }
                    
            return Json(new CountViewModel() { Count = count } );
        }

        private async Task<long> GetCountAsync(string serviceName)
        {
            string serviceUri = _serviceContext.CodePackageActivationContext.ApplicationName + "/" + serviceName;

            ServicePartitionList partitions = await _fabricClient.QueryManager.GetPartitionListAsync(new Uri(serviceUri));

            long count = 0;
            foreach (Partition partition in partitions)
            {
                long partitionKey = ((Int64RangePartitionInformation)partition.PartitionInformation).LowKey;
                IActorService actorServiceProxy = ActorServiceProxy.Create(new Uri(serviceUri), partitionKey);

                ContinuationToken continuationToken = null;

                do
                {
                    PagedResult<ActorInformation> page = await actorServiceProxy.GetActorsAsync(continuationToken, CancellationToken.None);

                    count += page.Items.Where(x => x.IsActive).LongCount();

                    continuationToken = page.ContinuationToken;
                }
                while (continuationToken != null);
            }

            return count;
        }

        // POST api/actorbackendservice
        [HttpPost]
        public async Task<IActionResult> PostAsync(string actorType)
        {
            switch (actorType)
            {
                case "netcore":
                    string serviceUriNetCore = _serviceContext.CodePackageActivationContext.ApplicationName + "/" + _configSettings.ActorBackendServiceNetCoreName;
                    INetCoreActor proxyNetCore = ActorProxy.Create<INetCoreActor>(ActorId.CreateRandom(), new Uri(serviceUriNetCore));
                    long x = await proxyNetCore.GetCountAsync(CancellationToken.None);
                    // await proxyNetCore.StartProcessingAsync(CancellationToken.None);
                    break;

                default:
                    string serviceUri = _serviceContext.CodePackageActivationContext.ApplicationName + "/" + _configSettings.ActorBackendServiceName;
                    IMyActor proxy = ActorProxy.Create<IMyActor>(ActorId.CreateRandom(), new Uri(serviceUri));
                    await proxy.StartProcessingAsync(CancellationToken.None);
                    break;
            }

            return Json(true);
        }
    }
}