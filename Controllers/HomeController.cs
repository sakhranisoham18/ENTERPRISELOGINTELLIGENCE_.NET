using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;
namespace EnterpriseLogIntelligence.Controllers;
[AllowAnonymous] public sealed class HomeController:Controller{public IActionResult Error()=>View();}
