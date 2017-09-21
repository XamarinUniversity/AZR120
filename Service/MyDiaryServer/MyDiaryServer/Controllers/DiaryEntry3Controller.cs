using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.Azure.Mobile.Server;
using MyDiaryServer.DataObjects;
using MyDiaryServer.Models;

namespace MyDiaryServer.Controllers
{
    [Authorize]
    [RoutePrefix("tables/diaryentry3")]
    public class DiaryEntry3Controller : TableController<DiaryEntry>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<DiaryEntry>(context, Request);
        }

        string GetUserId()
        {
            var user = User as ClaimsPrincipal;
            Claim nameClaim = user?.FindFirst(c => c.Type == ClaimTypes.NameIdentifier);
            return nameClaim?.Value;
        }

        // GET tables/SDiaryEntry
        public IQueryable<DiaryEntry> GetAllTodoItems()
        {
            string userId = GetUserId();
            return Query().Where(de => de.UserId == userId);
        }

        // GET tables/SDiaryEntry/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<DiaryEntry> GetTodoItem(string id)
        {
            string userId = GetUserId();
            return Query().SingleOrDefaultAsync(de => de.UserId == userId && de.Id == id);
        }

        // PATCH tables/SDiaryEntry/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<DiaryEntry> PatchTodoItem(string id, Delta<DiaryEntry> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/SDiaryEntry
        public async Task<IHttpActionResult> PostTodoItem(DiaryEntry item)
        {
            item.UserId = GetUserId();
            DiaryEntry current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/SDiaryEntry/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteTodoItem(string id)
        {
            var item = Lookup(id);
            var diaryEntry = item.Queryable.SingleOrDefault();
            if (diaryEntry?.UserId == GetUserId())
                return DeleteAsync(id);

            throw new HttpResponseException(System.Net.HttpStatusCode.BadRequest);
        }
    }
}