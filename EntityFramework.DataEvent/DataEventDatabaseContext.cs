using EntityFramework.DataEvent.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.DataEvent
{
    public class DataEventDatabaseContext : DbContext
    {
        private readonly IDataEventPublisher DataEventPublisher;
        private List<DataEventMessage> dataEvents;
        private List<object> added { get; set; } = new List<object>();
        private List<object> deleted { get; set; } = new List<object>();
        private List<object> modified { get; set; } = new List<object>();
        private List<object> accessed { get; set; } = new List<object>();
        public DataEventDatabaseContext(DbContextOptions options, IDataEventPublisher dataEventPublisher):base(options)
        {
            DataEventPublisher = dataEventPublisher;
            SavingChanges += Context_SavingChanges;
            SavedChanges += Context_SavedChanges; 
            
        }
        public DataEventDatabaseContext(IDataEventPublisher dataEventPublisher) 
        {
            DataEventPublisher = dataEventPublisher;
            SavingChanges += Context_SavingChanges;
            SavedChanges += Context_SavedChanges;
            SaveChangesFailed += Context_SavedChangesFailed;
            
        }

        private void Context_SavedChangesFailed(object? sender, SaveChangesFailedEventArgs e)
        {
            DataEventPublisher.OnTransactionFailed();
        }

        private void Context_SavedChanges(object sender, SavedChangesEventArgs e)
        {

            DataEventPublisher.OnTransactionCompleted();
        }

        private void Context_SavingChanges(object sender, SavingChangesEventArgs e)
        {
            string transactionId = Guid.NewGuid().ToString();
            DateTime transactionTime = DateTime.UtcNow;
            var added = ChangeTracker.Entries()
                .Where(
                    e => e.State == EntityState.Added)
                .Select(e => e.Entity)
                .ToList();

            var modified = ChangeTracker.Entries()
                .Where(
                    e => e.State == EntityState.Modified)
                .Select(e => e.Entity)
                .ToList();
            var deleted = ChangeTracker.Entries()
                            .Where(
                                e => e.State == EntityState.Deleted)
                            .Select(e => e.Entity)
                            .ToList();
            var accessed = ChangeTracker.Entries()
                .Where(
                    e => e.State == EntityState.Unchanged)
                .Select(e => e.Entity)
                .ToList();


            foreach (var docEntry in added)
            {
                DataEventPublisher.OnDataItemCreated(docEntry,transactionId);

            }
            foreach(var docEntry in modified)
            {
                DataEventPublisher.OnDataItemUpdated(docEntry, transactionId);

            }
            foreach(var docEntry in deleted)
            {
                DataEventPublisher.OnDataItemDeleted(docEntry, transactionId);

            }
            //foreach (var docEntry in modified)
            //{
            //    DbSet<DataChange> changes = GlobalDb.Set<DataChange>();
            //    var d = System.Text.Json.JsonSerializer.Serialize(docEntry);
            //    IBotRespondDataEvent? dataEvent = docEntry as IBotRespondDataEvent;
            //    if (dataEvent != null)
            //    {
            //        changes.Add(new DataChange(_requestContext, dataEvent, d, transactionTime));
            //    }
            //}
        }

        public override void Dispose()
        {
            SavingChanges -= Context_SavingChanges;
            SavedChanges -= Context_SavedChanges;
            SaveChangesFailed -= Context_SavedChangesFailed;
            base.Dispose();
        }

        public override ValueTask DisposeAsync()
        {
            SavingChanges -= Context_SavingChanges;
            SavedChanges -= Context_SavedChanges;
            SaveChangesFailed -= Context_SavedChangesFailed;
            return base.DisposeAsync();
        }
    }
}
