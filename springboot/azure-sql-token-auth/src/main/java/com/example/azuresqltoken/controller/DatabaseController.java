package com.example.azuresqltoken.controller;

import com.example.azuresqltoken.service.DatabaseService;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.sql.SQLException;
import java.util.List;
import java.util.Map;

@RestController
@RequestMapping("/api/data")
public class DatabaseController {
    private final DatabaseService databaseService;

    public DatabaseController(DatabaseService databaseService) {
        this.databaseService = databaseService;
    }

    @PostMapping("/{tableName}")
    public ResponseEntity<?> createRecord(
            @PathVariable String tableName,
            @RequestBody Map<String, Object> data) {
        try {
            databaseService.createRecord(tableName, data);
            return ResponseEntity.ok().build();
        } catch (SQLException e) {
            return ResponseEntity.internalServerError()
                    .body("Error creating record: " + e.getMessage());
        }
    }

    @GetMapping("/{tableName}")
    public ResponseEntity<?> readRecords(
            @PathVariable String tableName,
            @RequestParam(required = false) String condition) {
        try {
            List<Map<String, Object>> results = databaseService.readRecords(tableName, condition);
            return ResponseEntity.ok(results);
        } catch (SQLException e) {
            return ResponseEntity.internalServerError()
                    .body("Error reading records: " + e.getMessage());
        }
    }

    @PutMapping("/{tableName}")
    public ResponseEntity<?> updateRecord(
            @PathVariable String tableName,
            @RequestBody Map<String, Object> data,
            @RequestParam String condition) {
        try {
            int updatedCount = databaseService.updateRecord(tableName, data, condition);
            return ResponseEntity.ok(Map.of("recordsUpdated", updatedCount));
        } catch (SQLException e) {
            return ResponseEntity.internalServerError()
                    .body("Error updating record: " + e.getMessage());
        }
    }

    @DeleteMapping("/{tableName}")
    public ResponseEntity<?> deleteRecord(
            @PathVariable String tableName,
            @RequestParam String condition) {
        try {
            int deletedCount = databaseService.deleteRecord(tableName, condition);
            return ResponseEntity.ok(Map.of("recordsDeleted", deletedCount));
        } catch (SQLException e) {
            return ResponseEntity.internalServerError()
                    .body("Error deleting record: " + e.getMessage());
        }
    }
}