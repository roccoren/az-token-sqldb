package com.example.azuresqltoken.service;

import com.example.azuresqltoken.manager.ConnectionManager;
import org.springframework.stereotype.Service;
import org.springframework.beans.factory.annotation.Autowired;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

@Service
public class DatabaseService {
    private static final Logger logger = LoggerFactory.getLogger(DatabaseService.class);
    private final ConnectionManager connectionManager;

    @Autowired
    public DatabaseService(ConnectionManager connectionManager) {
        logger.info("Initializing DatabaseService");
        this.connectionManager = connectionManager;
    }

    public void createRecord(String tableName, Map<String, Object> data) throws SQLException {
        StringBuilder sql = new StringBuilder("INSERT INTO " + tableName + " (");
        StringBuilder values = new StringBuilder(") VALUES (");
        List<Object> parameters = new ArrayList<>();

        for (Map.Entry<String, Object> entry : data.entrySet()) {
            sql.append(entry.getKey()).append(", ");
            values.append("?, ");
            parameters.add(entry.getValue());
        }

        // Remove trailing commas and close the SQL statement
        sql.setLength(sql.length() - 2);
        values.setLength(values.length() - 2);
        sql.append(values).append(")");

        try (Connection conn = connectionManager.getConnection();
             PreparedStatement stmt = conn.prepareStatement(sql.toString())) {
            
            for (int i = 0; i < parameters.size(); i++) {
                stmt.setObject(i + 1, parameters.get(i));
            }
            
            stmt.executeUpdate();
        }
    }

    public List<Map<String, Object>> readRecords(String tableName, String condition) throws SQLException {
        String sql = "SELECT * FROM " + tableName;
        if (condition != null && !condition.trim().isEmpty()) {
            sql += " WHERE " + condition;
        }

        List<Map<String, Object>> results = new ArrayList<>();
        
        try (Connection conn = connectionManager.getConnection();
             PreparedStatement stmt = conn.prepareStatement(sql);
             ResultSet rs = stmt.executeQuery()) {

            while (rs.next()) {
                Map<String, Object> row = new HashMap<>();
                for (int i = 1; i <= rs.getMetaData().getColumnCount(); i++) {
                    String columnName = rs.getMetaData().getColumnName(i);
                    Object value = rs.getObject(i);
                    row.put(columnName, value);
                }
                results.add(row);
            }
        }

        return results;
    }

    public int updateRecord(String tableName, Map<String, Object> data, String condition) throws SQLException {
        StringBuilder sql = new StringBuilder("UPDATE " + tableName + " SET ");
        List<Object> parameters = new ArrayList<>();

        for (Map.Entry<String, Object> entry : data.entrySet()) {
            sql.append(entry.getKey()).append(" = ?, ");
            parameters.add(entry.getValue());
        }

        // Remove trailing comma and space
        sql.setLength(sql.length() - 2);

        if (condition != null && !condition.trim().isEmpty()) {
            sql.append(" WHERE ").append(condition);
        }

        try (Connection conn = connectionManager.getConnection();
             PreparedStatement stmt = conn.prepareStatement(sql.toString())) {
            
            for (int i = 0; i < parameters.size(); i++) {
                stmt.setObject(i + 1, parameters.get(i));
            }
            
            return stmt.executeUpdate();
        }
    }

    public int deleteRecord(String tableName, String condition) throws SQLException {
        String sql = "DELETE FROM " + tableName;
        if (condition != null && !condition.trim().isEmpty()) {
            sql += " WHERE " + condition;
        }

        try (Connection conn = connectionManager.getConnection();
             PreparedStatement stmt = conn.prepareStatement(sql)) {
            return stmt.executeUpdate();
        }
    }
}