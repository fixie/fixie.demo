namespace ContactList.Features.Contact
{
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    public class ContactController : BaseController
    {
        readonly IMediator _mediator;

        public ContactController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ActionResult> Index(ContactIndex.Query query)
        {
            var model = await _mediator.Send(query);

            return View(model);
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Add(AddContact.Command command)
        {
            if (ModelState.IsValid)
            {
                await _mediator.Send(command);

                SuccessMessage($"{command.Name} has been added.");

                return RedirectToAction("Index");
            }

            return View(command);
        }

        public async Task<ActionResult> Edit(EditContact.Query query)
        {
            var model = await _mediator.Send(query);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(EditContact.Command command)
        {
            if (ModelState.IsValid)
            {
                await _mediator.Send(command);

                SuccessMessage($"{command.Name} has been updated.");

                return RedirectToAction("Index");
            }

            return View(command);
        }

        [HttpPost]
        public async Task<ActionResult> Delete(DeleteContact.Command command)
        {
            if (ModelState.IsValid)
            {
                await _mediator.Send(command);

                SuccessMessage($"{command.Name} has been deleted.");

                return AjaxRedirect(Url.Action("Index"));
            }

            return BadRequest();
        }
    }
}