namespace ContactList.Features
{
    using Microsoft.AspNetCore.Mvc;

    public abstract class BaseController : Controller
    {
        protected JsonResult AjaxRedirect(string url)
        {
            return Json(new { redirectUrl = url });
        }

        protected void SuccessMessage(string message)
        {
            Toast(message, "success");
        }

        protected void ErrorMessage(string message)
        {
            Toast(message, "error");
        }

        void Toast(string message, string type)
        {
            TempData["ToastMessage"] = message;
            TempData["ToastType"] = type;
        }
    }
}