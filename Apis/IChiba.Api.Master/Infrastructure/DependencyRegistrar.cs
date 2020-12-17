using Autofac;
using IChiba.Caching;
using IChiba.Core.Configuration;
using IChiba.Core.Infrastructure;
using IChiba.Core.Infrastructure.DependencyManagement;
using IChiba.Data;
using IChiba.Services.Common;
using IChiba.Services.Master;
using IChiba.Web.Framework.Infrastructure.Extensions;

namespace IChiba.Api.Master.Infrastructure
{
    /// <summary>
    /// Dependency registrar
    /// </summary>
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, IChibaConfig config)
        {
            // Data Layer
            builder.RegisterDataConnection(new[]
            {
                DataConnectionHelper.ConnectionStringNames.Master
            });

            // Repositories
            builder.RegisterRepository(new[]
            {
                DataConnectionHelper.ConnectionStringNames.Master
            });

            // Cache
            builder.RegisterCacheManager();

            // Services
            builder.RegisterType<AirlineService>().As<IAirlineService>().InstancePerLifetimeScope();
            builder.RegisterType<BankService>().As<IBankService>().InstancePerLifetimeScope();
            builder.RegisterType<BankAccountService>().As<IBankAccountService>().InstancePerLifetimeScope();
            builder.RegisterType<ChargesGroupService>().As<IChargesGroupService>().InstancePerLifetimeScope();
            builder.RegisterType<ChargesTypeService>().As<IChargesTypeService>().InstancePerLifetimeScope();
            builder.RegisterType<CommodityService>().As<ICommodityService>().InstancePerLifetimeScope();
            builder.RegisterType<CommodityGroupService>().As<ICommodityGroupService>().InstancePerLifetimeScope();
            builder.RegisterType<ConsigneeService>().As<IConsigneeService>().InstancePerLifetimeScope();
            builder.RegisterType<CountryService>().As<ICountryService>().InstancePerLifetimeScope();
            builder.RegisterType<CurrencyService>().As<ICurrencyService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomAgentService>().As<ICustomAgentService>().InstancePerLifetimeScope();
            builder.RegisterType<DeliveryTimeService>().As<IDeliveryTimeService>().InstancePerLifetimeScope();
            builder.RegisterType<DistrictService>().As<IDistrictService>().InstancePerLifetimeScope();
            builder.RegisterType<EventTypeService>().As<IEventTypeService>().InstancePerLifetimeScope();
            builder.RegisterType<ForwardingAgentService>().As<IForwardingAgentService>().InstancePerLifetimeScope();
            builder.RegisterType<IncotermService>().As<IIncotermService>().InstancePerLifetimeScope();
            builder.RegisterType<MeasureDimensionService>().As<IMeasureDimensionService>().InstancePerLifetimeScope();
            builder.RegisterType<MeasureWeightService>().As<IMeasureWeightService>().InstancePerLifetimeScope();
            builder.RegisterType<PackageTypeService>().As<IPackageTypeService>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentMethodService>().As<IPaymentMethodService>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentTermService>().As<IPaymentTermService>().InstancePerLifetimeScope();
            builder.RegisterType<PortService>().As<IPortService>().InstancePerLifetimeScope();
            builder.RegisterType<PostOfficeService>().As<IPostOfficeService>().InstancePerLifetimeScope();
            builder.RegisterType<ShipperService>().As<IShipperService>().InstancePerLifetimeScope();
            builder.RegisterType<ShippingAgentService>().As<IShippingAgentService>().InstancePerLifetimeScope();
            builder.RegisterType<ShippingLineService>().As<IShippingLineService>().InstancePerLifetimeScope();
            builder.RegisterType<SPAddressService>().As<ISPAddressService>().InstancePerLifetimeScope();
            builder.RegisterType<SPCustomerService>().As<ISPCustomerService>().InstancePerLifetimeScope();
            builder.RegisterType<SPMeasurementService>().As<ISPMeasurementService>().InstancePerLifetimeScope();
            builder.RegisterType<SPMoveTypeService>().As<ISPMoveTypeService>().InstancePerLifetimeScope();
            builder.RegisterType<SPProductTypeService>().As<ISPProductTypeService>().InstancePerLifetimeScope();
            builder.RegisterType<SPSpecialServiceTypeService>().As<ISPSpecialServiceTypeService>().InstancePerLifetimeScope();
            builder.RegisterType<StateProvinceService>().As<IStateProvinceService>().InstancePerLifetimeScope();
            builder.RegisterType<VesselService>().As<IVesselService>().InstancePerLifetimeScope();
            builder.RegisterType<TruckerService>().As<ITruckerService>().InstancePerLifetimeScope();
            builder.RegisterType<VatTypeService>().As<IVatTypeService>().InstancePerLifetimeScope();
            builder.RegisterType<VendorService>().As<IVendorService>().InstancePerLifetimeScope();
            builder.RegisterType<WardService>().As<IWardService>().InstancePerLifetimeScope();
            builder.RegisterType<GlobalZoneService>().As<IGlobalZoneService>().InstancePerLifetimeScope();
            builder.RegisterType<WarehouseService>().As<IWarehouseService>().InstancePerLifetimeScope();
        }

        /// <summary>
        /// Gets order of this dependency registrar implementation
        /// </summary>
        public int Order => 2;
    }
}
