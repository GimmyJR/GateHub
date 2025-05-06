
using FirebaseAdmin;
using GateHub.Hubs;
using GateHub.Models;
using GateHub.repository;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

namespace GateHub
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            

            builder.Services.AddControllers();
            //builder.Services.AddControllers().AddJsonOptions(options =>
            //{
            //    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
            //});
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            
            builder.Services.AddIdentity<AppUser, IdentityRole>()
            .AddEntityFrameworkStores<GateHubContext>()
            .AddDefaultTokenProviders();
            
            builder.Services.AddSignalR();
            
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy =>
                    {
                        //policy.WithOrigins("http://127.0.0.1:5500")
                        //       .AllowAnyHeader()
                        //       .AllowAnyMethod()
                        //       .AllowCredentials();
                        policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
            });

            builder.Services.AddScoped<IAdminRepo, AdminRepo>();
            builder.Services.AddScoped<IVehicleOwnerRepo, VehicleOwnerRepo>();
            builder.Services.AddScoped<IGateStaffRepo, GateStaffRepo>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IGenerateTokenService, GenerateTokenService>();
            builder.Services.AddScoped<ISystemFeatures, SystemFeatures>() ;

            builder.Services.AddScoped<FirebaseNotificationService>();

            builder.Services.AddDbContext<GateHubContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            
            builder.Services.AddHttpClient<PaymobService>();
            
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    ClockSkew = TimeSpan.Zero
                };
            });
            
            builder.Services.AddAuthorization();

            var firebaseCredentialPath = Path.Combine(AppContext.BaseDirectory, "firebase-adminsdk.json");

            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(firebaseCredentialPath)
            });


            var app = builder.Build();

            //if (app.Environment.IsDevelopment())
            //{
                app.UseSwagger();
                app.UseSwaggerUI();
            //}
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseWebSockets();
            app.UseAuthorization();
            
            app.MapHub<NotificationHub>("/notificationHub");

            app.MapControllers();

            app.Run();
        }
    }
}
