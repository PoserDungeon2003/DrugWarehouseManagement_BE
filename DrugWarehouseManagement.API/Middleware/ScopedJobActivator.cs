using Hangfire;

namespace DrugWarehouseManagement.API.Middleware
{
    public class ScopedJobActivator : JobActivator  
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public ScopedJobActivator(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new ScopedJobActivatorScope(_scopeFactory.CreateScope());
        }
    }
    public class ScopedJobActivatorScope : JobActivatorScope
    {
        private readonly IServiceScope _scope;
        public ScopedJobActivatorScope(IServiceScope scope)
        {
            _scope = scope;
        }
        public override object Resolve(Type type)
        {
            return _scope.ServiceProvider.GetRequiredService(type);
        }
        public override void DisposeScope()
        {
            _scope.Dispose();
        }
    }
}
