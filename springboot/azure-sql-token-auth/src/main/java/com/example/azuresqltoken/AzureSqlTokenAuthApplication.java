package com.example.azuresqltoken;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.autoconfigure.jdbc.DataSourceAutoConfiguration;

/**
 * Main application class for Azure SQL Token Authentication demo
 * Excludes default datasource auto-configuration since we're using custom token-based authentication
 */
@SpringBootApplication(exclude = {DataSourceAutoConfiguration.class})
public class AzureSqlTokenAuthApplication {
    public static void main(String[] args) {
        SpringApplication.run(AzureSqlTokenAuthApplication.class, args);
    }
}