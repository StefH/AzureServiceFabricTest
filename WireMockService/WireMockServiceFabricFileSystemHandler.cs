using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WireMock.Handlers;

namespace WireMockService
{
    internal class WireMockServiceFabricFileSystemHandler : IFileSystemHandler
    {
        private readonly IReliableStateManager _reliableStateManager;

        internal WireMockServiceFabricFileSystemHandler(IReliableStateManager reliableStateManager)
        {
            _reliableStateManager = reliableStateManager;
        }

        public void CreateFolder(string path)
        {
            // just return
        }

        public IEnumerable<string> EnumerateFiles(string path)
        {
            // var data = await _reliableStateManager.GetOrAddAsync<IReliableDictionary<string, string>>("WireMockServiceFabricFileSystemHandlerReliableDictionary");

            //data

            //using (var tx = _reliableStateManager.CreateTransaction())
            //{
            //    var result = await data..TryGetValueAsync(tx, "Counter");

            //    ServiceEventSource.Current.ServiceMessage(Context, "Current Counter Value: {0}", result.HasValue ? result.Value.ToString() : "Value does not exist.");

            //    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

            //    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
            //    // discarded, and nothing is saved to the secondary replicas.
            //    await tx.CommitAsync();
            //}

            return new string[] {};
        }

        public bool FolderExists(string path)
        {
            // just return true always
            return true;
        }

        public string GetMappingFolder()
        {
            throw new System.NotImplementedException();
        }

        public string ReadMappingFile(string path)
        {
            throw new System.NotImplementedException();
        }

        public void WriteMappingFile(string path, string text)
        {
            throw new System.NotImplementedException();
        }
    }
}
