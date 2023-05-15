using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using MiniaturesGallery.Models.Abstracts;
using Microsoft.AspNetCore.Identity;

namespace MiniaturesGallery.Autorization
{
    public class IsOwnerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, OwnedAbs>
    {
        UserManager<IdentityUser> _userManager;

        public IsOwnerAuthorizationHandler(UserManager<IdentityUser>
            userManager)
        {
            _userManager = userManager;
        }

        protected override Task
            HandleRequirementAsync(AuthorizationHandlerContext context,
                                   OperationAuthorizationRequirement requirement,
                                   OwnedAbs resource)
        {
            if (context.User == null || resource == null)
            {
                return Task.CompletedTask;
            }

            // If not asking for CRUD permission, return.

            if (requirement.Name != HelpClasses.Constants.CreateOperationName &&
                requirement.Name != HelpClasses.Constants.UpdateOperationName &&
                requirement.Name != HelpClasses.Constants.DeleteOperationName)
            {
                return Task.CompletedTask;
            }

            if (resource.UserID == _userManager.GetUserId(context.User))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
