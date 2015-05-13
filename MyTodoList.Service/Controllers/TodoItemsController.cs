using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using MyTodoList.Service.Filters;
using MyTodoList.Service.Persistence;
using MyTodoList.Shared.Models;

namespace MyTodoList.Service.Controllers
{
    public class TodoItemsController : ApiController
    {
        private MyTodoListServiceContext db = new MyTodoListServiceContext();

        // GET: api/TodoItems
        [IdentityTokenAuthentication]
        public IQueryable<TodoItem> GetTodoItems()
        {
            string authenticatedCallerId = GetAuthenticatedId();
            return db.TodoItems.Where(item => item.OwnerId == authenticatedCallerId);
        }

        // GET: api/TodoItems/5
        [ResponseType(typeof(TodoItem))]
        [IdentityTokenAuthentication]
        public IHttpActionResult GetTodoItem(int id)
        {
            TodoItem todoItem = db.TodoItems.Find(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            if (todoItem.OwnerId != GetAuthenticatedId())
            {
                return new ForbiddenResult(Request);
            }

            return Ok(todoItem);
        }

        // PUT: api/TodoItems/5
        [ResponseType(typeof(void))]
        [IdentityTokenAuthentication]
        public IHttpActionResult PutTodoItem(string id, TodoItem todoItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!TodoItemExists(id))
            {
                return NotFound();
            }

            // Only allow modification of items that the caller owns
            if (todoItem.OwnerId != GetAuthenticatedId())
            {
                return new ForbiddenResult(Request);
            }

            // Don't allow id to be changed
            todoItem.Id = id;

            db.Entry(todoItem).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TodoItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/TodoItems
        [ResponseType(typeof(TodoItem))]
        [IdentityTokenAuthentication]
        public IHttpActionResult PostTodoItem(TodoItem todoItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ignore caller-provided Id and create a new unique ID
            todoItem.Id = Guid.NewGuid().ToString();

            // Ignore caller-provided Id and OwnerId (user our own values instead) 
            todoItem.OwnerId = GetAuthenticatedId();

            db.TodoItems.Add(todoItem);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = todoItem.Id }, todoItem);
        }

        // DELETE: api/TodoItems/5
        [ResponseType(typeof(TodoItem))]
        [IdentityTokenAuthentication]
        public IHttpActionResult DeleteTodoItem(string id)
        {
            TodoItem todoItem = db.TodoItems.Find(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            // Only allow deleting of items that the caller owns
            if (todoItem.OwnerId != GetAuthenticatedId())
            {
                return new ForbiddenResult(Request);
            }

            db.TodoItems.Remove(todoItem);
            db.SaveChanges();

            return Ok(todoItem);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TodoItemExists(string id)
        {
            return db.TodoItems.Count(e => e.Id == id) > 0;
        }

        private string GetAuthenticatedId()
        {
            var principal = RequestContext.Principal as ClaimsPrincipal;
            if (principal == null)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("RequestContext.Principal is null.  This should never happen. BTW, if you call GetAuthenticatedId, you must use IdentityTokenAuthentication attribute :-)")
                });
            }

            return principal.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier).Value;
        }
    }

    public class ForbiddenResult : IHttpActionResult
    {
        private readonly HttpRequestMessage _request;
        private readonly string _reason;

        public ForbiddenResult(HttpRequestMessage request, string reason)
        {
            _request = request;
            _reason = reason;
        }

        public ForbiddenResult(HttpRequestMessage request)
        {
            _request = request;
            _reason = "Forbidden";
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = _request.CreateResponse(HttpStatusCode.Forbidden, _reason);
            return Task.FromResult(response);
        }
    }
}
