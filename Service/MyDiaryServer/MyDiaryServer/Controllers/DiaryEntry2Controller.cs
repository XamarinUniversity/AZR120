using System.Linq;
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
    [RoutePrefix("tables/diaryentry2")]
    public class DiaryEntry2Controller : TableController<DiaryEntry>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<DiaryEntry>(context, Request);
        }

        // GET tables/SDiaryEntry
        public IQueryable<DiaryEntry> GetAllTodoItems()
        {
            return Query();
        }

        // GET tables/SDiaryEntry/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<DiaryEntry> GetTodoItem(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/SDiaryEntry/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<DiaryEntry> PatchTodoItem(string id, Delta<DiaryEntry> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/SDiaryEntry
        public async Task<IHttpActionResult> PostTodoItem(DiaryEntry item)
        {
            DiaryEntry current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/SDiaryEntry/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteTodoItem(string id)
        {
            return DeleteAsync(id);
        }
    }
}