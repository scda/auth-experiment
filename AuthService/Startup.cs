using System.Threading.Tasks;
using authservice.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Prometheus;

namespace authservice
{
    public class Startup
    {
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            _environment = environment;
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddAuthorizationCore();

            services.AddAuthentication(options =>
                    {
                        // options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        // options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultAuthenticateScheme =
                            CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultSignInScheme =
                            CookieAuthenticationDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme =
                            OpenIdConnectDefaults.AuthenticationScheme;
                    }).AddCookie()
                    .AddOpenIdConnect("oidc", options =>
                    {
                        options.RequireHttpsMetadata = false; // required for non-https keycloak
                        options.Authority = Configuration["Security:Oidc:Authority"];
                        options.ClientId = "authserviceclient";
                        options.MetadataAddress = Configuration["Security:Oidc:MetadataAddress"]; // will read config directly from keycloak
                        //options.ClientSecret = "secret"; // not required for authorization code flow
                        options.ResponseType = OpenIdConnectResponseType.Code;
                        options.SaveTokens = true;
                        options.GetClaimsFromUserInfoEndpoint = true;
                        options.UseTokenLifetime = false;
                        options.UsePkce = true;
                        options.Scope.Add("openid");
                        options.Scope.Add("profile");
                        options.TokenValidationParameters = new
                            TokenValidationParameters
                            {
                                NameClaimType = "name"
                            };

                        options.Events = new OpenIdConnectEvents
                        {
                            OnAccessDenied = context =>
                            {
                                context.HandleResponse();
                                context.Response.Redirect("/");
                                return Task.CompletedTask;
                            }
                        };
                        // .AddJwtBearer(o =>
                        // {
                        //     o.Authority = Configuration["Security:Oidc:Authority"]; // TODO: use options pattern
                        //     o.Audience = Configuration["Security:Oidc:Audience"];
                        //     o.Events = new JwtBearerEvents
                        //     {
                        //         OnAuthenticationFailed = c =>
                        //         {
                        //             c.NoResult();
                        //
                        //             c.Response.StatusCode = 500;
                        //             return c.Response.WriteAsync(_environment.IsDevelopment()
                        //                 ? c.Exception.ToString()
                        //                 : "An error occured processing your authentication.");
                        //         }
                        //     };
                        // }
                        // );
                    });

            services.AddSingleton<WeatherForecastService>();
            services.AddHealthChecks()
                    .ForwardToPrometheus();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHttpMetrics(options =>
            {
                // This identifies the page when using Razor Pages.
                options.AddRouteParameter("page");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapMetrics();
                endpoints.MapHealthChecks("/healthz")
                         .RequireHost($"*:{Configuration["ManagementPort"]}");
            });
        }
    }
}
