using System;
using System.Linq;
using System.Threading.Tasks;
using Ietws;
using Microsoft.Extensions.Options;

namespace Sloth.Core.Services
{
    public class IamDirectorySearchService: IDirectorySearchService
    {
        private readonly IetClient _ietClient;

        public IamDirectorySearchService(IOptions<IamDirectorySearchServiceOptions> options)
        {
            var apiKey = options.Value.ApiKey;
            _ietClient = new IetClient(apiKey);
        }


        public async Task<DirectoryUser> GetByEmail(string email)
        {
            // find the contact via their email
            var ucdContactResult = await _ietClient.Contacts.Search(ContactSearchField.email, email);
            EnsureResponseSuccess(ucdContactResult);
            if (ucdContactResult.ResponseData.Results.Length == 0)
            {
                return null;
            }
            var ucdContact = ucdContactResult.ResponseData.Results.First();

            // now look up the whole person's record by ID including kerb
            var ucdKerbResult = await _ietClient.Kerberos.Get(ucdContact.IamId);
            EnsureResponseSuccess(ucdKerbResult);
            if (ucdKerbResult.ResponseData.Results.Length == 0)
            {
                return null;
            }
            var ucdKerbPerson = ucdKerbResult.ResponseData.Results.Single();

            return TransformKerberosResult(ucdKerbPerson, ucdContact);
        }

        public async Task<DirectoryUser> GetByKerberos(string kerb)
        {
            // find the contact via their kerb
            var ucdKerbResult = await _ietClient.Kerberos.Search(KerberosSearchField.userId, kerb);
            EnsureResponseSuccess(ucdKerbResult);
            if (ucdKerbResult.ResponseData.Results.Length == 0)
            {
                return null;
            }
            var ucdKerbPerson = ucdKerbResult.ResponseData.Results.Single();

            // find their email
            var ucdContactResult = await _ietClient.Contacts.Get(ucdKerbPerson.IamId);
            EnsureResponseSuccess(ucdContactResult);
            if (ucdContactResult.ResponseData.Results.Length == 0)
            {
                return null;
            }
            var ucdContact = ucdContactResult.ResponseData.Results.First();

            return TransformKerberosResult(ucdKerbPerson, ucdContact);
        }

        private static DirectoryUser TransformKerberosResult(KerberosResult ucdKerbPerson, ContactResult ucdContact)
        {
            return new DirectoryUser
            {
                GivenName = ucdKerbPerson.DFirstName,
                Surname   = ucdKerbPerson.DLastName,
                FullName  = ucdKerbPerson.DFullName,
                Kerberos  = ucdKerbPerson.UserId,
                Email     = ucdContact.Email
            };
        }

        private static void EnsureResponseSuccess<T>(IetResult<T> result)
        {
            if (result.ResponseStatus != 0)
            {
                throw new ApplicationException(result.ResponseDetails);
            }
        }
    }

    public class IamDirectorySearchServiceOptions
    {
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; }
    }
}
