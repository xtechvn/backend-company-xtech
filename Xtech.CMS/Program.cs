using Entities.ConfigModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Repositories;
using Repositories.IRepositories;
using Repositories.Repositories;
using System.Text.Json;
using Ultilities;
using Ultilities.RedisWorker;
using WEB.CMS.Customize;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

builder.Services.AddSession();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.AccessDeniedPath = new PathString("/Account/RedirectLogin");
    options.LoginPath = new PathString("/Account/RedirectLogin");
    options.ReturnUrlParameter = "url";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // nếu dùng ExpireTimeSpan thì  SlidingExpiration phải set là false. Như vậy cho dù tương tác hay k tương tác thì đều timeout theo thời gian đã set
    options.SlidingExpiration = true; //được sử dụng để thiết lập thời gian sống của cookie dựa trên thời gian cuối cùng mà người dùng đã tương tác với ứng dụng . Nếu người dùng tiếp tục tương tác với ứng dụng trước khi cookie hết hạn, thời gian sống của cookie sẽ được gia hạn thêm.

    options.Cookie = new CookieBuilder
    {
        HttpOnly = true,
        Name = "Net.Security.Cookie",
        Path = "/",
        SameSite = SameSiteMode.Lax,
        SecurePolicy = CookieSecurePolicy.SameAsRequest
    };

});
ConfigurationManager configuration = builder.Configuration; // allows both to access and to set up the config
// Add services to the container.
builder.Services.AddSingleton<IImagesConvertRepository, ImagesConvertRepository>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.Configure<DataBaseConfig>(configuration.GetSection("DataBaseConfig"));
builder.Services.Configure<MailConfig>(configuration.GetSection("MailConfig"));
builder.Services.Configure<DomainConfig>(configuration.GetSection("DomainConfig"));

// Register services
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<IPaymentVoucherRepository, PaymentVoucherRepository>();

builder.Services.AddSingleton<IAllCodeRepository, AllCodeRepository>();
builder.Services.AddSingleton<ICommonRepository, CommonRepository>();
builder.Services.AddTransient<ICustomerManagerRepository, CustomerManagerRepository>();
builder.Services.AddTransient<IAccountClientRepository, AccountClientRepository>();
builder.Services.AddTransient<IClientRepository, ClientRepository>();
builder.Services.AddTransient<IBankingAccountRepository, BankingAccountRepository>();
builder.Services.AddTransient<IUserAgentRepository, UserAgentRepository>();
builder.Services.AddSingleton<IMenuRepository, MenuRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IRoleRepository, RoleRepository>();
builder.Services.AddTransient<IPermissionRepository, PermissionRepository>();
builder.Services.AddTransient<IPositionRepository, PositionRepository>();
builder.Services.AddTransient<IAllCodeRepository, AllCodeRepository>();
builder.Services.AddTransient<IAttachFileRepository, AttachFileRepository>();
builder.Services.AddTransient<IMFARepository, MFARepository>();
builder.Services.AddTransient<IArticleRepository, ArticleRepository>();
builder.Services.AddTransient<IDashboardRepository, DashboardRepository>();
builder.Services.AddTransient<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddTransient<IGroupProductRepository, GroupProductRepository>();
builder.Services.AddTransient<IIdentifierServiceRepository, IdentifierServiceRepository>();
builder.Services.AddTransient<IOrderRepository, OrderRepository>();
builder.Services.AddTransient<IPaymentAccountRepository, PaymentAccountRepository>();
builder.Services.AddTransient<IInvoiceRequestRepository, InvoiceRequestRepository>();
builder.Services.AddTransient<IInvoiceRequestDetailRepository, InvoiceRequestDetailRepository>();
builder.Services.AddTransient<IOrderRepositor, OrderRepositor>();
builder.Services.AddTransient<IDepositHistoryRepository, DepositHistoryRepository>();
builder.Services.AddTransient<IDebtGuaranteeRepository, DebtGuaranteeRepository>();
builder.Services.AddTransient<ISupplierRepository, SupplierRepository>();
builder.Services.AddTransient<ITicketRepository, TicketRepository>();
builder.Services.AddTransient<ISprintRepository, SprintRepository>();
builder.Services.AddTransient<IProjectTaskRepository, ProjectTaskRepository>();
builder.Services.AddTransient<IProjectRepository, ProjectRepository>();

//-- API:
builder.Services.AddTransient< IArticleAPIRepository, ArticleAPIRepository> ();
builder.Services.AddTransient< IGroupProductAPIRepository, GroupProductAPIRepository> ();
builder.Services.AddTransient< ITagRepository, TagRepository> ();
builder.Services.AddTransient< IBookingVPSRepository, BookingVPSRepository> ();
builder.Services.AddTransient<IOtherBookingRepository, OtherBookingRepository>();
builder.Services.AddTransient<IOtherBookingPackageRepository, OtherBookingPackageRepository>();
builder.Services.AddTransient<IContractPayRepository, ContractPayRepository>();
builder.Services.AddTransient<IPaymentRequestRepository, PaymentRequestRepository>();
builder.Services.AddTransient<ITelegramRepository, TelegramRepository>();


// Setting Redis                     
builder.Services.AddSingleton<RedisConn>();
builder.Services.AddSingleton<ManagementUser>();
builder.Services.AddHttpClient(); // ← thêm dòng này
// ✅ SignalR camelCase cho payload
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PayloadSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebUserCors", p =>
        p.SetIsOriginAllowed(_ => true)   // dev test nhanh
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()
    );
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseSession();

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
// app.UseAntiXssMiddleware();
app.UseRouting();
app.UseCors("WebUserCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<TicketHub>("/ticketHub");


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "Order",
    pattern: "/OrderDetail/{orderId?}",
    defaults: new { controller = "Order", action = "OrderDetail" });
app.MapControllerRoute(
    name: "setupManual",
                 pattern: "/product/setup-manual",
                 defaults: new { controller = "product", action = "SetupManual" });
app.MapControllerRoute(name: "transactionsms",
  pattern: "/transactionsms",
  defaults: new { controller = "TransactionSms", action = "Index" });
app.MapControllerRoute(name: "Order",
 pattern: "/Order/{id?}",
 defaults: new { controller = "Order", action = "Orderdetails" });
app.MapControllerRoute(name: "SetService",
 pattern: "SetService/fly/detail/{group_booking_id}",
 defaults: new { controller = "SetService", action = "FlyDetail" });
app.MapControllerRoute(name: "SetService",
 pattern: "SetService/Tour/Detail/{id}",
 defaults: new { controller = "SetService", action = "TourDetail" });
app.MapControllerRoute(name: "SetService",
pattern: "SetService/Others/Detail/{id}",
defaults: new { controller = "SetService", action = "OtherDetail" });
app.MapControllerRoute(name: "SetService",
pattern: "SetService/VinWonder/Detail/{id}",
defaults: new { controller = "SetService", action = "VinWonderDetail" });


app.MapControllerRoute(name: "AccountSetup",
pattern: "/Account/2FA",
defaults: new { controller = "Account", action = "Setup2FA" });
app.MapControllerRoute(name: "ProgramsPackage",
pattern: "/ProgramsPackage/DetailListProgramsPackage/{id}/{Packageid}/{ProgramName}/{RoomTypeid}",
defaults: new { controller = "ProgramsPackage", action = "DetailListProgramsPackage" });
app.MapControllerRoute(name: "ProgramsPackage",
pattern: "/ProgramsPackage/AddListProgramsPackage/{id}/{Packageid}/{ProgramName}/{RoomTypeid}/{type}",
defaults: new { controller = "ProgramsPackage", action = "AddListProgramsPackage" });
app.MapControllerRoute(name: "ProgramsPackage",
pattern: "/ProgramsPackage/ProgramsPriceHotelIndex",
defaults: new { controller = "ProgramsPackage", action = "ProgramsPriceHotelIndex" });

app.MapControllerRoute(name: "RequestHotelBooking",
pattern: "/RequestHotelBooking/Detail/{hotel_booking_id}/{ClientId}/{id}",
defaults: new { controller = "RequestHotelBooking", action = "Detail" });

app.Run();
