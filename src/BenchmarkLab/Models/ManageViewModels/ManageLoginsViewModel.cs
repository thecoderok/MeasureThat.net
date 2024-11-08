using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

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
