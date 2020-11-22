using System.DirectoryServices.AccountManagement;

//dotnet add package System.DirectoryServices.AccountManagement
namespace EZInventory.InfoClasses{

	class ADQuerier {

        public ADQuerier() {

        }
		
        public string GetDisplayName(string username) {

            string domain = "";

            try {
                domain = System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain().Name;
            } 
            catch {
                return ""; //Couldn't get domain info
			}
            
            //Remove domain prefix from a username mydomain\alexm -> alexm
            string domainPrefix = domain.ToLower().Substring(0,domain.LastIndexOf("."));
            if (username.ToLower().Contains(domainPrefix+"\\")) { //IE domain\user
                username = username.Remove(0,username.LastIndexOf("\\")+1); //+1 to remove slashes
            } 

            PrincipalContext AD = new PrincipalContext(ContextType.Domain, domain);
            UserPrincipal u = new UserPrincipal(AD);  
            u.SamAccountName = username;  

            PrincipalSearcher search = new PrincipalSearcher(u);  
            UserPrincipal result = (UserPrincipal)search.FindOne();  
            search.Dispose();  
  
            // show some details  
            //Console.WriteLine("Display Name : " + result.DisplayName);  
            //Console.WriteLine("Phone Number : " + result.VoiceTelephoneNumber);
            if (result != null) {
                return result.DisplayName;
            }
            else {
                return "?????";
            }
            
        }
		
	}

}