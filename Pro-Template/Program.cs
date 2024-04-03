using Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Pro_Template.Data;
using Pro_Template.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(
    options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddIdentityApiEndpoints<CustomUser>(options =>
{
    options.Password = new PasswordOptions
    {
        RequireDigit = false,
        RequiredLength = 1,
        RequiredUniqueChars = 0,
        RequireLowercase = false,
        RequireNonAlphanumeric = false,
        RequireUppercase = false
    };
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.ChangePhoneNumberTokenProvider = TokenOptions.DefaultEmailProvider;
})
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.ConfigureApplicationCookie(options => { options.Cookie.SameSite = SameSiteMode.None; });

// email
// Use SMTP4DEV to test email sending
builder.Services.AddFluentEmail(builder.Configuration);
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapIdentityApi<CustomUser>();

// seems like signout is missing... 
app.MapPost("/logout", async (SignInManager<CustomUser> signInManager) =>
{
    await signInManager.SignOutAsync().ConfigureAwait(false);
}).RequireAuthorization();

app.Run();
