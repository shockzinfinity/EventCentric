﻿using EventCentric;
using EventCentric.Queueing;
using EventCentric.Utils;
using Microsoft.Practices.Unity;

namespace EasyTrade.EmpresasQueue
{
    public class EmpresasQueueNodeInitializer : NodeInitializer
    {
        public static void Initialize(IUnityContainer container)
        {
            Initialize(
                () =>
                {
                    var node = QueueNodeFactory<EmpresasQueueApp>
                        .CreateCrudNode<EmpresasQueueDbContext>(container);

                    System.Data.Entity.Database.SetInitializer<EmpresasQueueDbContext>(null);

                    var app = new EmpresasQueueApp(container.Resolve<ICrudEventQueue>(), container.Resolve<IGuidProvider>(), container.Resolve<ITimeProvider>());

                    // For asp.net controller dependency injection
                    container.RegisterInstance<IEmpresasQueueApp>(app);

                    return node;
                });
        }
    }
}
