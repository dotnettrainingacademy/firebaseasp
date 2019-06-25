using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using WebApplication5.Models;

namespace WebApplication5.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _env;
        private static string ApiKey = "AIzaSyC_bHn5KFYDuJZge38qGEsbPLPJWj8YD-I";
        private static string Bucket = "tieupworld.appspot.com";
        private static string AuthEmail = "rajaraman6195@gmail.com";
        private static string AuthPassword = "Flutter@123";
        public HomeController(IHostingEnvironment env)
        {
            _env = env;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(FileUploadViewModel model)
        {
            var file = model.File;
            FileStream fs;
            FileStream ms;
            if (file.Length > 0)
            {
                string folderName = "Favicon";
                string path = Path.Combine(_env.WebRootPath, $"images/{folderName}");
                ms = new FileStream(Path.Combine(path, file.FileName), FileMode.Open);
                var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

                // you can use CancellationTokenSource to cancel the upload midway
                var cancellation = new CancellationTokenSource();

                var task = new FirebaseStorage(
                    Bucket,
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                        ThrowOnCancel = true // when you cancel the upload, exception is thrown. By default no exception is thrown
                    })
                    .Child("receipts")
                    .Child("test")
                    .Child($"aspcore.png")
                    .PutAsync(ms, cancellation.Token);

                task.Progress.ProgressChanged += (s, e) => Console.WriteLine($"Progress: {e.Percentage} %");

                // cancel the upload
                // cancellation.Cancel();

                try
                {
                    // error during upload will be thrown when you await the task
                    Console.WriteLine("Download link:\n" + await task);
                }
                catch (Exception ex)
                {
                    ViewBag.error = $"Exception was thrown: {ex}";
                }

            }


            return BadRequest();
        }

        private static async Task Run(FileStream fs)
        {

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
