using ECPSPdfSignerV2.Model;
using ECPSPdfSignerV2.Model.DTO;
using ECPSPdfSignerV2.Services;
using EcpsService.Models.DTO;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;

namespace ECPSPdfSignerV2.Data
{
    public class APIService
    {
        private readonly HttpClient _client;
        private readonly ILogger<APIService> _logger;
        
        private CustomAuthenticationStateProvider _authStateProvider;
        public APIService(CustomAuthenticationStateProvider authStateProvider, ILogger<APIService> logger)
        {
            _client = new HttpClient();
            _authStateProvider = authStateProvider;
            _logger = logger;   
        }

        public async Task<bool> LoginVerify(LoginModel loginModel)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(loginModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = await _client.PostAsync(SD._baseURL + SD._loginURL, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync();
                    var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<TokenResponseModel>(responseContent.Result);
                    if(tokenResponse != null)
                    {
                        await _authStateProvider.Login(tokenResponse.accessToken, tokenResponse.userId);

                        var userInfo = await GetUserInfo(new Guid(tokenResponse.userId));
                        SD._UserRoles = userInfo.roles;
                        SD._CurrentUserRole = SD._UserRoles.FirstOrDefault() ?? string.Empty;  
                        SD._UserName = userInfo.name;
                        return true;
                    }
                    return false;
                }
                else
                {
                    _logger.LogInformation("Status Code: {statusCode}; Login() API failed. Request-Name: {userName}; Request-Password: {password}.", response.StatusCode, loginModel.Username, loginModel.Password);
                    return false;
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "UserID: {userID} Name: {userName} Role: {userRole}.", await SecureStorage.GetAsync("userID"), SD._UserName, SD._CurrentUserRole);
                return false;
            }
        }

        public async Task Logout()
        {
            await _authStateProvider.Logout();
        }

        public bool InternetConnection()
        {
            var currentStatus = Connectivity.NetworkAccess;
            return currentStatus == NetworkAccess.Internet;
        }

        public async Task<List<SignPendingFileDTO>?> GetPendingFilesAsync()
        {
            var files = new List<SignPendingFileDTO>();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync("accountToken"));

            try
            {
                HttpResponseMessage response = await _client.GetAsync(SD._baseURL + SD._pendingFilesURL);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var result = System.Text.Json.JsonSerializer.Deserialize<FileToSignResponseModel>(responseContent);
                    files = result != null ? result.data : files;
                }
                else
                {
                    _logger.LogInformation("Status Code: {statusCode} GetPendingFiles() API failed. UserID: {userID} Name: {userName} Role: {userRole}.", response.StatusCode, await SecureStorage.GetAsync("userID"), SD._UserName, SD._CurrentUserRole);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserID: {userID} Name: {userName} Role: {userRole}.", await SecureStorage.GetAsync("userID"), SD._UserName, SD._CurrentUserRole);
            }

            return files ;
        }
        
        public async Task<List<SignPendingFileDTO>?> GetSignedFilesAsync()
        {
            var files = new List<SignPendingFileDTO>();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync("accountToken"));

            try
            {
                HttpResponseMessage response = await _client.GetAsync(SD._baseURL + SD._signedFilesURL);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var result = System.Text.Json.JsonSerializer.Deserialize<FileToSignResponseModel>(responseContent);
                    files = result != null? result.data : files;
                }
                else
                {
                    _logger.LogInformation("Status Code: {statusCode} GetSignedFiles() API failed. UserID: {userID} Name: {userName} Role: {userRole}.", response.StatusCode, await SecureStorage.GetAsync("userID"), SD._UserName, SD._CurrentUserRole);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserID: {userID} Name: {userName} Role: {userRole}.", await SecureStorage.GetAsync("userID"), SD._UserName, SD._CurrentUserRole);
            }

            return files;
        } 

        public async Task<string?> DownloadDocument(string fileNo)
        {
            // Delete all the files in 'Documents\Rajuk Files' folder
            Utility.ClearFolder();
            try
            {
                HttpResponseMessage response = await _client.GetAsync(SD._baseURL + SD._fileDownloadURL + fileNo);

                if (response.IsSuccessStatusCode)
                {
                    // Read content from response
                    byte[] documentBytes = await response.Content.ReadAsByteArrayAsync();
                    var fileName = fileNo + ".pdf";
                    var filePath = Path.Combine(Utility.GetFolderPath(), fileName);
                    await File.WriteAllBytesAsync(filePath, documentBytes);

                    return fileName;
                }
                else
                {
                    _logger.LogInformation("Status Code: {statusCode} File: {fileNo} download failed. UserID: {userID} Name: {userName} Role: {userRole}.", response.StatusCode, fileNo, await SecureStorage.GetAsync("userID"), SD._UserName, SD._CurrentUserRole);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File: {fileNo} download error. UserID: {userID} Name: {userName} Role: {userRole}.", fileNo, await SecureStorage.GetAsync("userID"), SD._UserName, SD._CurrentUserRole);
                return null;
            }
        }

        public async Task<bool> UploadSignedPDF(string localFilePath, string fileNo)
        {
            try
            {
                using (var formData = new MultipartFormDataContent())
                {
                    // Read the PDF file into a byte array
                    byte[] fileBytes = File.ReadAllBytes(localFilePath);

                    var fileContent = new ByteArrayContent(fileBytes);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");

                    formData.Add(fileContent, "file", "_" + fileNo + ".pdf");
                    formData.Add(new StringContent(fileNo), "fileNo");

                    // Post the form data to the server
                    HttpResponseMessage response = await _client.PostAsync(SD._baseURL + SD._fileUploadURL, formData);
                    string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else
                    {
                        _logger.LogInformation("Status Code: {statusCode} File: {fileNo} Upload API failed. UserID: {userID} Name: {userName} Role: {userRole}.", response.StatusCode, fileNo, await SecureStorage.GetAsync("userID"), SD._UserName, SD._CurrentUserRole);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading PDF for fileNo: {fileNo}. UserID: {userID} Name: {userName} Role: {userRole}.", fileNo, await SecureStorage.GetAsync("userID"), SD._UserName, SD._CurrentUserRole);
                return false;
            }
        }

		public async Task<List<MembersDesignationFlowDTO>?> GetMembersDesignationFlowAsync(Guid subzoneid)
		{
			var UsersFlow = new List<MembersDesignationFlowDTO>();
			_client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync("accountToken"));

            try
            {
                var values = new Dictionary<string, string>{{ "subzoneID", subzoneid.ToString() }};
                var json = System.Text.Json.JsonSerializer.Serialize(values);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(SD._baseURL + SD._MembersDesignationFlow, content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var result = System.Text.Json.JsonSerializer.Deserialize<MembersDesignationFlowParseDTO>(responseContent);
                    UsersFlow = result != null ? result.data : UsersFlow;
                }
                else
                {
                    _logger.LogInformation("Status Code: {statusCode} GetMembersDesignation() API failed. UserID: {userID} Name: {userName} Role: {userRole}.", response.StatusCode, await SecureStorage.GetAsync("userID"), SD._UserName, SD._CurrentUserRole);
                }
            }
			catch (Exception ex)
			{
                _logger.LogError(ex, "UserID: {userID} Name: {userName} Role: {userRole}.", await SecureStorage.GetAsync("userID"), SD._UserName, SD._CurrentUserRole);
            }

            return UsersFlow;
		}

        public async Task<UserInfoDTO> GetUserInfo(Guid userid)
        {
            UserInfoDTO? userInfo  = new();
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync("accountToken"));

            try
            {
                var values = new Dictionary<string, Guid> { { "userID", userid } };
                var json = System.Text.Json.JsonSerializer.Serialize(values);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(SD._baseURL + SD._UserInfoURL, content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    userInfo = System.Text.Json.JsonSerializer.Deserialize<UserInfoDTO>(responseContent);
                }
                else
                {
                    _logger.LogInformation("Status Code: {statusCode} GetUserRoles() API failed. UserID: {userID} Name: {userName} Role: {userRole}.", response.StatusCode, await SecureStorage.GetAsync("userID"), SD._UserName, SD._CurrentUserRole);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserID: {userID} Name: {userName} Role: {userRole}.", await SecureStorage.GetAsync("userID"), SD._UserName, SD._CurrentUserRole);
            }

            return userInfo ?? new UserInfoDTO();
        }
        
        public async Task<bool> SelectUserRole(Guid userid, string role)
        {
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", await SecureStorage.GetAsync("accountToken"));

            try
            {
                var values = new Dictionary<string, string> { { "userID", userid.ToString() }, { "roleName", role } };
                var json = System.Text.Json.JsonSerializer.Serialize(values);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _client.PostAsync(SD._baseURL + SD._SetUserRoleURL, content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    SD._CurrentUserRole = role;
                    return true;
                }
                else
                {
                    _logger.LogInformation("Status Code: {statusCode} SelectUserRole() API failed. UserID: {userID} Name: {userName} Role: {userRole}.", response.StatusCode, await SecureStorage.GetAsync("userID"), SD._UserName, SD._CurrentUserRole);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserID: {userID} Name: {userName} Role: {userRole}.", await SecureStorage.GetAsync("userID"), SD._UserName, SD._CurrentUserRole);
            }

            return false;
        }
    }
}