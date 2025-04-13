package com.example.azuresqltoken.manager;

import com.zaxxer.hikari.HikariConfig;
import com.zaxxer.hikari.HikariDataSource;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

import org.springframework.context.annotation.Configuration;
import jakarta.annotation.PostConstruct;
import jakarta.annotation.PreDestroy;
import javax.sql.DataSource;
import org.springframework.context.annotation.Bean;
import java.sql.Connection;
import java.sql.SQLException;

@Configuration
public class ConnectionManager {
    private static final Logger logger = LoggerFactory.getLogger(ConnectionManager.class);
    private HikariDataSource dataSource;
    private final TokenManager tokenManager;

    @Value("${azure.sql.server-name:}")
    private String serverName;

    @Value("${azure.sql.database-name:}")
    private String databaseName;

    @Autowired
    public ConnectionManager(TokenManager tokenManager) {
        this.tokenManager = tokenManager;
        logger.info("ConnectionManager created with TokenManager");
        if (tokenManager == null) {
            logger.error("TokenManager is null - dependency injection failed");
            throw new IllegalStateException("TokenManager is required but not injected");
        }
    }

    @PostConstruct
    public void init() {
        logger.info("Initializing ConnectionManager with server: {}, database: {}", serverName, databaseName);
        
        HikariConfig config = new HikariConfig();
        config.setPoolName("SQLTokenPool");
        config.setMaximumPoolSize(10);
        config.setMinimumIdle(5);
        config.setConnectionTimeout(30000);
        config.setIdleTimeout(600000);
        config.setMaxLifetime(1800000);

        // Use SQLServerDataSource for more robust configuration
        config.setDataSourceClassName("com.microsoft.sqlserver.jdbc.SQLServerDataSource");
        
        // Configure Azure SQL Database properties
        String token = getAccessToken();
        logger.debug("Configuring Azure SQL connection properties");
        config.addDataSourceProperty("serverName", serverName);
        config.addDataSourceProperty("databaseName", databaseName);
        config.addDataSourceProperty("accessToken", token);
        
        // Configure encryption and certificate validation
        config.addDataSourceProperty("encrypt", "true");
        config.addDataSourceProperty("trustServerCertificate", "false");
        config.addDataSourceProperty("hostNameInCertificate", "*.database.windows.net");
        
        logger.debug("Connection properties configured successfully");
        try {
            this.dataSource = new HikariDataSource(config);
            logger.info("Connection pool initialized successfully");
            
            // Verify connection can be established
            try (Connection testConn = dataSource.getConnection()) {
                logger.info("Test connection successful - connection is valid");
            } catch (SQLException se) {
                logger.error("Test connection failed - connection is not valid", se);
                throw se;
            }
        } catch (Exception e) {
            logger.error("Failed to initialize connection pool", e);
            throw new RuntimeException("Failed to initialize connection pool", e);
        }
    }

    private String getAccessToken() {
        try {
            String token = tokenManager.getToken();
            logger.debug("Access token obtained - length: {}, starts with: {}", 
                token != null ? token.length() : 0,
                token != null && token.length() > 10 ? token.substring(0, 10) + "..." : "null");
            return token;
        } catch (Exception e) {
            logger.error("Failed to get access token", e);
            throw new RuntimeException("Failed to get access token", e);
        }
    }

    public Connection getConnection() throws SQLException {
        return dataSource.getConnection();
    }

    public void releaseConnection(Connection connection) {
        if (connection != null) {
            try {
                connection.close();
            } catch (SQLException e) {
                logger.error("Error releasing connection", e);
            }
        }
    }

    @PreDestroy
    public void shutdown() {
        if (dataSource != null && !dataSource.isClosed()) {
            dataSource.close();
        }
    }
}