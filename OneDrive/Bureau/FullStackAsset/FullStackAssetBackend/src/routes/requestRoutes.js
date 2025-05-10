/**
 * @swagger
 * tags:
 *   name: Requests
 *   description: API for requests management
 */

/**
 * @swagger
 * /requests:
 *   post:
 *     summary: Create a new request
 *     tags: [Requests]
 *     security:
 *       - bearerAuth: []
 *     requestBody:
 *       required: true
 *       content:
 *         application/json:
 *           schema:
 *             type: object
 *             properties:
 *               title:
 *                 type: string
 *               description:
 *                 type: string
 *               priority:
 *                 type: string
 *                 enum: [low, medium, high]
 *               department:
 *                 type: string
 *     responses:
 *       201:
 *         description: Request created
 *       400:
 *         description: Bad request
 *   get:
 *     summary: Get all requests
 *     tags: [Requests]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: List of requests
 *
 * /requests/export:
 *   get:
 *     summary: Export all requests to Excel
 *     tags: [Requests]
 *     security:
 *       - bearerAuth: []
 *     responses:
 *       200:
 *         description: Excel file
 */
const express = require('express');
const router = express.Router();
const requestController = require('../controllers/requestController');
const { protect, authorize } = require('../middleware/auth');

// Protect all routes
router.use(protect);

// Admin only routes
router.post('/', authorize('ADMIN'), requestController.createRequest);
router.get('/', authorize('ADMIN'), requestController.getAllRequests);
router.get('/export', authorize('ADMIN'), requestController.exportToExcel);

module.exports = router; 