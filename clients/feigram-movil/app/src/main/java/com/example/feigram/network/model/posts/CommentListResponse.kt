package com.example.feigram.network.model.posts

import com.google.gson.annotations.SerializedName

data class CommentListResponse(
    @SerializedName("post_id")
    val postId: String,
    @SerializedName("comments")
    val comments: List<Comment>
)