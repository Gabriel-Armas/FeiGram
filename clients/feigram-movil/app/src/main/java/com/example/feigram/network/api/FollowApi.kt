package com.example.feigram.network.api

import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.POST
import retrofit2.http.Path

interface FollowApi {

    @GET("/follow/is_following/{follower}/{followed}")
    suspend fun isFollowing(
        @Path("follower") follower: String,
        @Path("followed") followed: String,
        @Header("Authorization") token: String
    ): Map<String, Boolean>

    @POST("/follow/follow/{follower}/{followed}")
    suspend fun followUser(
        @Path("follower") follower: String,
        @Path("followed") followed: String,
        @Header("Authorization") token: String
    ): Map<String, String>

    @DELETE("/follow/unfollow/{follower}/{followed}")
    suspend fun unfollowUser(
        @Path("follower") follower: String,
        @Path("followed") followed: String,
        @Header("Authorization") token: String
    ): Map<String, String>
}
