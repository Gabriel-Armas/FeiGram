package com.example.feigram.network.service

import android.annotation.SuppressLint
import okhttp3.*
import org.json.JSONObject
import java.security.SecureRandom
import java.security.cert.X509Certificate
import javax.net.ssl.*

object WebSocketManager {
    private var webSocket: WebSocket? = null
    private val client = getUnsafeOkHttpClient()
    private var currentToken: String? = null
    private val listeners = mutableListOf<(String) -> Unit>()

    var isConnected: Boolean = false
        private set

    fun connect(token: String) {
        if (webSocket != null) {
            webSocket?.cancel()
            webSocket = null
        }

        currentToken = token

        val request = Request.Builder()
            .url("wss://10.0.2.2/messages/ws/?token=$token")
            .build()

        webSocket = client.newWebSocket(request, object : WebSocketListener() {
            override fun onOpen(webSocket: WebSocket, response: Response) {
                isConnected = true
                println("WebSocket conectado")
            }

            override fun onMessage(webSocket: WebSocket, text: String) {
                println("Mensaje recibido: $text")
                listeners.forEach { it(text) }
            }

            override fun onFailure(webSocket: WebSocket, t: Throwable, response: Response?) {
                isConnected = false
                println("WebSocket error: ${t.localizedMessage}")
            }

            override fun onClosing(webSocket: WebSocket, code: Int, reason: String) {
                isConnected = false
                webSocket.close(1000, null)
                println("WebSocket cerrando: $reason")
            }
        })
    }

    fun reconnectIfNeeded(token: String) {
        if (!isConnected) {
            println("WebSocket no conectado, intentando reconectar...")
            connect(token)
        }
    }

    fun addListener(listener: (String) -> Unit) {
        listeners.add(listener)
    }

    fun removeListener(listener: (String) -> Unit) {
        listeners.remove(listener)
    }

    fun sendJsonMessage(json: JSONObject) {
        webSocket?.send(json.toString())
    }

    fun sendTextMessage(text: String) {
        webSocket?.send(text)
    }

    fun disconnect() {
        webSocket?.close(1000, "App closed")
        webSocket = null
        isConnected = false
        listeners.clear()
    }

    private fun getUnsafeOkHttpClient(): OkHttpClient {
        try {
            val trustAllCerts = arrayOf<TrustManager>(
                @SuppressLint("CustomX509TrustManager")
                object : X509TrustManager {
                    @SuppressLint("TrustAllX509TrustManager")
                    override fun checkClientTrusted(chain: Array<X509Certificate>, authType: String) {}
                    @SuppressLint("TrustAllX509TrustManager")
                    override fun checkServerTrusted(chain: Array<X509Certificate>, authType: String) {}
                    override fun getAcceptedIssuers(): Array<X509Certificate> = arrayOf()
                }
            )

            val sslContext = SSLContext.getInstance("SSL")
            sslContext.init(null, trustAllCerts, SecureRandom())
            val sslSocketFactory = sslContext.socketFactory

            return OkHttpClient.Builder()
                .sslSocketFactory(sslSocketFactory, trustAllCerts[0] as X509TrustManager)
                .hostnameVerifier { _, _ -> true }
                .build()
        } catch (e: Exception) {
            throw RuntimeException(e)
        }
    }
}
