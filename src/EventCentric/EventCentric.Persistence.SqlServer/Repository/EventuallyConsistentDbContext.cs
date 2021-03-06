﻿using System;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace EventCentric.Persistence.SqlServer
{
    public class EventuallyConsistentDbContext : EventStoreDbContext
    {
        private readonly TimeSpan timeout;

        public EventuallyConsistentDbContext(TimeSpan timeout, bool isReadOnly, string connectionString)
            : base(isReadOnly, connectionString)
        {
            this.timeout = timeout;
        }

        public EventuallyConsistentDbContext(string connectionString)
            : base(connectionString)
        { }

        public IDbSet<EventuallyConsistentResult> EventuallyConsistentResults { get; set; }

        public EventuallyConsistentResult AwaitEventualConsistency(Guid transactionId)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.Elapsed < this.timeout)
            {
                if (this.EventuallyConsistentResults.Any(r => r.TransactionId == transactionId))
                    return this.EventuallyConsistentResults
                                .Where(r => r.TransactionId == transactionId)
                                .OrderByDescending(r => r.Id)
                                .First();

                Thread.Sleep(100);
            }

            throw new TimeoutException(string.Format("Timeout while awaiting eventual consistency for transaccion id {0}. Timeout: {1} seconds.", transactionId.ToString(), this.timeout.TotalSeconds));
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new EventuallyConsistentResultEntityMap());
        }
    }
}
