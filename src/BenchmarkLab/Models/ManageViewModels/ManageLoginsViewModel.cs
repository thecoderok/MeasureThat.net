using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace MeasureThat.Net.Models.ManageViewModels
{
    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins
        {
            get; set;
        }

        public IList<AuthenticationScheme> OtherLogins
        {
            get; set;
        }
    }
}
