package com.example.feigram.network.api

import com.example.feigram.network.model.feed.FeedRecommendationsResponse
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.Query

interface FeedApi {
    @GET("feed/posts/recommendations")
    suspend fun getRecommendations(
        @Header("Authorization") token: String,
        @Query("user_id") userId: String,
        @Query("skip") skip: Int = 0,
        @Query("limit") limit: Int = 5
    ): FeedRecommendationsResponse
}
