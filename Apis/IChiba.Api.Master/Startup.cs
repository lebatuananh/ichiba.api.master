using System.Collections.Generic;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using IChiba.Core;
using IChiba.Core.Configuration;
using IChiba.Core.Infrastructure;
using IChiba.Core.Infrastructure.Mapper;
using IChiba.Data;
using IChiba.SharedMvc.Infrastructure.AutoMapperProfiles;
using IChiba.Web.Framework.Infrastructure;
using IChiba.Web.Framework.Infrastructure.Extensions;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace IChiba.Api.Master
{
    public class Startup
    {
        private IEngine _engine;
        private IChibaConfig _config;

        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
#if DEBUG
            var ichibaRootPath = Path.GetFullPath(Path.Combine(webHostEnvironment.ContentRootPath, @"..\..\.."));
#else
            var ichibaRootPath = Path.GetFullPath(Path.Combine(webHostEnvironment.ContentRootPath, @".."));
#endif
            Configuration = CommonHelper.AddWebConfiguration(webHostEnvironment, ichibaRootPath);

            WebHostEnvironment = webHostEnvironment;

            CommonHelper.Configuration = configuration;
            CommonHelper.WebHostEnvironment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; private set; }

        public IWebHostEnvironment WebHostEnvironment { get; private set; }

        public ILifetimeScope AutofacContainer { get; private set; }

        // ConfigureServices is where you register dependencies. This gets
        // called by the runtime before the ConfigureContainer method, below.
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var ssoConfig = Configuration.GetSection(SSOConfig.SSO).Get<SSOConfig>();

            AutoMapperConfiguration.Profiles.AddRange(new Profile[]
            {
                new CommonProfile(),
                new MasterProfile()
            });

            (_engine, _config) = services.ConfigureApplicationServices(Configuration, WebHostEnvironment);

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // DataConnections
            services.AddIChibaDbContext(Configuration, new[]
            {
                DataConnectionHelper.ConnectionStringNames.Master
            });

            // Cache
            services.AddIChibaCaching(Configuration);

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = ssoConfig.Authority;
                    options.ApiName = ssoConfig.ApiName;
                    options.RequireHttpsMetadata = ssoConfig.RequireHttpsMetadata;
                });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "eFex Master Api",
                    Version = "v1"
                });

                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });

                c.CustomSchemaIds(x => x.FullName);
            });

            services.AddHealthChecks();
        }

        // ConfigureContainer is where you can register things directly
        // with Autofac. This runs after ConfigureServices so the things
        // here will override registrations made in ConfigureServices.
        // Don't build the container; that gets done for you by the factory.
        public void ConfigureContainer(ContainerBuilder builder)
        {
            _engine.RegisterDependencies(builder, _config);

            // Register your own things directly with Autofac, like:
            builder.RegisterModule(new CoreModule(EngineContext.Current.TypeFinder));
            //builder.RegisterModule(new ElasticsearchModule());
            //builder.RegisterModule(new MongoDbModule());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            // TODO-IChiba-Log
            //CommonHelper.AddLogger(configuration);

            // TODO-IChiba-ELK
            //app.UseAllElasticApm(Configuration);

            // If, for some reason, you need a reference to the built container, you
            // can use the convenience extension method GetAutofacRoot.
            AutofacContainer = app.ApplicationServices.GetAutofacRoot();

            app.ConfigureRequestPipeline(Configuration);

            app.StartEngine();

            app.UseSwagger()
               .UseSwaggerUI(c =>
               {
                   c.SwaggerEndpoint("/swagger/v1/swagger.json", "eFex Master Api");
                   c.DocumentTitle = "eFex Master Api";
               });

            app.UseHealthChecks("/health");
        }
    }
}
