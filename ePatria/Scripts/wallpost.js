/*  It's user control for wall operations like to add/display status and comments. 
*   Developed By: Brij Mohan
*   Website: http://techbrij.com
*   Developed On: 29 May 2013
*  
*/


var postApiUrl = '/api/WallPost/', commentApiUrl = '/api/Comment/', getPostUrl = '/api/WallPost/' + localStorage.apmId;


// Model
function Post(data) {
    var self = this;
    data = data || {};
    self.PostId = data.PostId;
    self.Message = ko.observable(data.Message || "");
    self.PostedBy = data.PostedBy || "";
    self.PostedByName = data.PostedByName || "";
    self.PostedByAvatar = data.PostedByAvatar || "";
    self.PostedDate = getTimeAgo(data.PostedDate);
    self.error = ko.observable();
    self.PostComments = ko.observableArray();

    self.newCommentMessage = ko.observable();
    self.addComment = function () {
        var comment = new Comment();
        comment.PostId = self.PostId;
        comment.Message(self.newCommentMessage());
        return $.ajax({
            url: commentApiUrl,
            dataType: "json",
            contentType: "application/json",
            cache: false,
            type: 'POST',
            data: ko.toJSON(comment)
        })
       .done(function (result) {
           self.PostComments.push(new Comment(result));
           self.newCommentMessage('');
       })
       .fail(function () {
           error('unable to add post');
       });
    }



    //self.addReview = function () {
    //    var review = new ReviewRelationDetail();
    //    review.PostId = self.PostId;
    //    review.ReviewRelationMasterID(self.newTest());
    //    return $.ajax({
    //        url: reviewApiUrl,
    //        dataType: "json",
    //        contentType: "application/json",
    //        cache: false,
    //        type: 'POST',
    //        data: ko.toJSON(review)
    //    })
    //   .done(function (result) {
    //       self.ReviewRelationDetails.push(new ReviewRelationDetail(result));
    //       self.newMessage('');
    //   })
    //   .fail(function () {
    //       error('unable to add post');
    //   });
    //}
        if (data.PostComments) {
            var mappedPosts = $.map(data.PostComments, function (item) { return new Comment(item); });
            self.PostComments(mappedPosts);
        }
        self.toggleComment = function (item, event) {
            $(event.target).next().find('.publishComment').toggle();
        }
    


        //if (data.ReviewRelationDetails) {
        //    var mappedPosts = $.map(data.ReviewRelationDetails, function (item) { return new ReviewRelationDetail(item); });
        //    self.ReviewRelationDetails(mappedPosts);
        //}
        //self.toggleReviewRelationDetail = function (item, event) {
        //    $(event.target).next().find('.publishReviewRelationDetail').toggle();
    //}

     
}



function Comment(data) {
    var self = this;
    data = data || {};

    // Persisted properties
    self.CommentId = data.CommentId;
    self.PostId = data.PostId;
    self.Message = ko.observable(data.Message || "");
    self.CommentedBy = data.CommentedBy || "";
    self.CommentedByAvatar = data.CommentedByAvatar || "";
    self.CommentedByName = data.CommentedByName || "";
    self.CommentedDate = getTimeAgo(data.CommentedDate);
    self.error = ko.observable();
    //persist edits to real values on accept
    self.deleteComment = function () {
        
    }
}

//function ReviewRelationDetail(data) {
//    var self = this;
//    data = data || {};

//    // Persisted properties
//    self.ReviewRelationDetailID = data.ReviewRelationDetailID;
//    self.ReviewRelationMasterID = data.ReviewRelationMasterID;
//    self.PostId = data.PostId;
//    self.error = ko.observable();
//    //persist edits to real values on accept
//    self.deleteComment = function () {

//    }
//}

function getTimeAgo(varDate) {
    if (varDate) {
        return $.timeago(varDate.toString().slice(-1) == 'Z' ? varDate : varDate+'Z');
    }
    else {
        return '';
    }
}


function viewModel() {
    var self = this;
    self.posts = ko.observableArray();
    self.newMessage = ko.observable();
    self.apmId = ko.observable(localStorage.apmId);
    self.error = ko.observable();
    self.loadPosts = function () {
        //To load existing posts
        $.ajax({
            url: getPostUrl,
            dataType: "json",
            contentType: "application/json",
            cache: false,
            type: 'GET',
        })
            .done(function (data) {
                var mappedPosts = $.map(data, function (item) { return new Post(item); });
                self.posts(mappedPosts);
            })
            .fail(function () {
                error('unable to load posts');
            });
    }

    self.addPost = function () {
        var post = new Post();
        post.Message(self.newMessage()+';'+self.apmId());
        return $.ajax({
            url: postApiUrl,
            dataType: "json",
            contentType: "application/json",
            cache: false,
            type: 'POST',
            data:  ko.toJSON(post)
        })
       .done(function (result) {           
           self.posts.splice(0,0,new Post(result));
           self.newMessage('');
         })
       .fail(function () {
                error('unable to add post');
       });
    }


    self.loadPosts();
    return self;

    //self.addReview = function () {
    //    var review = new ReviewRelationDetail();
    //    review.PostId = self.PostId;

    //    return $.ajax({
    //        url: reviewApiUrl,
    //        dataType: "json",
    //        contentType: "application/json",
    //        cache: false,
    //        type: 'POST',
    //        data: ko.toJSON(review)
    //    })
    //   .done(function (result) {
    //       self.ReviewRelationDetails.push(new ReviewRelationDetail(result));
    //       self.newMessage('');
    //   })
    //   .fail(function () {
    //       error('unable to add post');
    //    });

    //if (data.ReviewRelationDetails) {
    //    var mappedPosts = $.map(data.ReviewRelationDetails, function (item) { return new ReviewRelationDetail(item); });
    //    self.ReviewRelationDetails(mappedPosts);
    //}
    //self.toggleReviewRelationDetail = function (item, event) {
    //    $(event.target).next().find('.publishReviewRelationDetail').toggle();
    //}
};

//custom bindings

//textarea autosize
ko.bindingHandlers.jqAutoresize = {
    init: function (element, valueAccessor, aBA, vm) {
        if (!$(element).hasClass('msgTextArea')) {
            $(element).css('height', '1em');
        }
        $(element).autosize();
    }
};

ko.applyBindings(new viewModel());

