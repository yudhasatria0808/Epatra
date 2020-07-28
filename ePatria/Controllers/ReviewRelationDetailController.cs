using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using ePatria.Models;
using Microsoft.AspNet.Identity;

namespace ePatria.Controllers
{
    public class ReviewRelationDetailController : ApiController
    {

        //private string imgFolder = "/Images/profileimages/";
        //private string defaultAvatar = "user.png";
        private ePatriaDefault db = new ePatriaDefault();

        // GET api/ReviewRelationDetail
        public IEnumerable<ReviewRelationDetail> GetReviewRelationDetails()
        {
            var reviewrelationdetails = db.ReviewRelationDetails.Include(p => p.Posts);
            return reviewrelationdetails.AsEnumerable();
        }

        // GET api/ReviewRelationDetail/5
        public ReviewRelationDetail GetReviewRelationDetail(int id)
        {
            ReviewRelationDetail reviewRelationDetail = db.ReviewRelationDetails.Find(id);
            if (reviewRelationDetail == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return reviewRelationDetail;
        }

        // PUT api/ReviewRelationDetail/5
        public HttpResponseMessage PutReviewRelationDetail(int id, ReviewRelationDetail reviewrelationdetail)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != reviewrelationdetail.ReviewRelationDetailID)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(reviewrelationdetail).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/ReviewRelationDetail
        public HttpResponseMessage PostReviewRelationDetail(ReviewRelationDetail reviewRelationDetail, Post post)
        {
            reviewRelationDetail.ReviewRelationMasterID = 1;

            if (!ModelState.IsValid)
            {
                
                db.ReviewRelationDetails.Add(reviewRelationDetail);
                db.SaveChanges();

                var ret = new
                {
                    PostId = reviewRelationDetail.PostId,
                    ReviewRelationMasterID = reviewRelationDetail.ReviewRelationMasterID,
                    reviewRelationDetailID = reviewRelationDetail.ReviewRelationDetailID
                };

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, ret);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = reviewRelationDetail.PostId }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        public HttpResponseMessage DeleteReviewRelationDetail(int id)
        {
            ReviewRelationDetail reviewRelationDetail = db.ReviewRelationDetails.Find(id);
            if (reviewRelationDetail == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.ReviewRelationDetails.Remove(reviewRelationDetail);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, reviewRelationDetail);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        //private bool ReviewRelationDetailExists(int id)
        //{
        //    return db.ReviewRelationDetails.Count(e => e.ReviewRelationDetailID == id) > 0;
        //}
    }
}