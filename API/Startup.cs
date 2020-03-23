using Application.Activities;
using API.Middleware;
using Domain;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence;
using Infrastructure.Security;
using Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace API
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      // Dodavanje DbContext klase, baze podataka
      services.AddDbContext<DataContext>(opt =>
      {
        opt.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
      });
      // Dodavanje CORS, cross origin, da client-app moze da komunicira sa API
      services.AddCors(opt =>
      {
        opt.AddPolicy("CorsPolicy", policy =>
              {
                policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:3000");
              });
      });
      // dovoljno da se kaze samo za jedan handler, AddMediatR trazi samo assembly
      services.AddMediatR(typeof(List.Handler).Assembly);
      // AddFluentValidation za validaciju propertija
      services.AddControllers(opt =>
      {
        // dodavanje autorizacije za svaki controller
        var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        opt.Filters.Add(new AuthorizeFilter(policy));
      })
          .AddFluentValidation(cfg => cfg.RegisterValidatorsFromAssemblyContaining<Create>());


      // PROBLEM SA OVIM KODOM, dodavanje identity
      // var builder = services.AddIdentityCore<AppUser>();
      // var identityBuilder = new IdentityBuilder(builder.UserType, builder.Services);
      // identityBuilder.AddEntityFrameworkStores<DataContext>();
      // identityBuilder.AddSignInManager<SignInManager<AppUser>>();

      // zamena za prosli kod, dodavanje Identity 
      services.AddDefaultIdentity<AppUser>().AddEntityFrameworkStores<DataContext>();

      // dodavanje autentifikacije
      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["TokenKey"]));
      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opt =>
        {
          opt.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateAudience = false,
            ValidateIssuer = false
          };
        });

      // dodato za validaciju tokenom
      services.AddScoped<IJwtGenerator, JwtGenerator>();
      services.AddScoped<IUserAccessor, UserAccessor>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
      app.UseMiddleware<ErrorHandlingMiddleware>();
      if (env.IsDevelopment())
      {
        //app.UseDeveloperExceptionPage();
      }

      // iskljuceno dok se radi u Development, da sajt ne bi pitao za sigurnost
      // izbrisati iz launchSettings.json API: "applicationUrl": https://...., ostaviti samo http
      // OBAVEZNO VRATITI KAD JE PUBLISH
      // app.UseHttpsRedirection();

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseCors("CorsPolicy");
      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}