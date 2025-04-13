package com.example.azuresqltoken.manager;

import com.microsoft.aad.msal4j.*;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.scheduling.annotation.Scheduled;
import org.springframework.stereotype.Component;
import jakarta.annotation.PostConstruct;

import java.util.Collections;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.*;

@Component
public class TokenManager {
    private static final Logger logger = LoggerFactory.getLogger(TokenManager.class);
    private final Map<String, String> tokenPool;
    
    @Value("${azure.activedirectory.client-id}")
    private String clientId;
    
    @Value("${azure.activedirectory.client-secret}")
    private String clientSecret;
    
    @Value("${azure.activedirectory.tenant-id}")
    private String tenantId;

    @Value("${azure.sql.token-refresh-interval:25}")
    private int tokenRefreshInterval;

    public TokenManager() {
        this.tokenPool = new ConcurrentHashMap<>();
    }

    @PostConstruct
    public void init() {
        logger.info("Initializing TokenManager with client-id: {}", clientId);
        logger.info("Token refresh interval set to {} minutes", tokenRefreshInterval);
        if (clientId == null || clientSecret == null || tenantId == null) {
            logger.error("Missing required Azure AD configuration");
            throw new IllegalStateException("Missing required Azure AD configuration");
        }
        try {
            refreshToken(); // Initial token fetch
        } catch (Exception e) {
            logger.error("Failed to fetch initial token", e);
            throw new IllegalStateException("Failed to fetch initial token", e);
        }
    }

    @Scheduled(fixedRateString = "${azure.sql.token-refresh-interval:25}000") // Convert minutes to milliseconds
    public void scheduledTokenRefresh() {
        try {
            logger.debug("Scheduled token refresh starting...");
            refreshToken();
            logger.info("Token refreshed successfully");
        } catch (Exception e) {
            logger.error("Failed to refresh token in scheduled refresh", e);
        }
    }

    public String getToken() throws Exception {
        logger.debug("Getting token...");
        String currentToken = tokenPool.get("sqlToken");
        if (currentToken != null && validateToken(currentToken)) {
            logger.debug("Using existing valid token");
            return currentToken;
        }
        logger.debug("No valid token found, refreshing...");
        return refreshToken();
    }

    public synchronized String refreshToken() throws Exception {
        String spn = "https://database.windows.net/";
        String stsUrl = String.format("https://login.microsoftonline.com/%s/", tenantId);
        String scope = spn + "/.default";
        Set<String> scopes = Collections.singleton(scope);

        logger.debug("Acquiring new token from Azure AD...");
        ExecutorService executorService = Executors.newSingleThreadExecutor();
        IClientCredential credential = ClientCredentialFactory.createFromSecret(clientSecret);
        
        ConfidentialClientApplication clientApplication = ConfidentialClientApplication
            .builder(clientId, credential)
            .executorService(executorService)
            .authority(stsUrl)
            .build();

        CompletableFuture<IAuthenticationResult> future = clientApplication
            .acquireToken(ClientCredentialParameters.builder(scopes).build());

        String accessToken = future.get().accessToken();
        tokenPool.put("sqlToken", accessToken);
        
        executorService.shutdown();
        logger.info("Successfully acquired new token");
        return accessToken;
    }

    private boolean validateToken(String token) {
        // TODO: Implement token validation logic
        // This should check if the token is not expired and is valid
        return false; // For now, always refresh token
    }
}