const Request = require('../models/Request');
const ExcelJS = require('exceljs');

// Create a new request
exports.createRequest = async (req, res) => {
  try {
    const { title, description, priority, department } = req.body;
    const request = new Request({
      title,
      description,
      priority,
      department,
      createdBy: req.user._id
    });

    await request.save();
    res.status(201).json({
      success: true,
      data: request
    });
  } catch (error) {
    res.status(400).json({
      success: false,
      error: error.message
    });
  }
};

// Get all requests
exports.getAllRequests = async (req, res) => {
  try {
    const requests = await Request.find()
      .populate('createdBy', 'firstName lastName email')
      .sort({ createdAt: -1 });

    res.status(200).json({
      success: true,
      data: requests
    });
  } catch (error) {
    res.status(500).json({
      success: false,
      error: error.message
    });
  }
};

// Export requests to Excel
exports.exportToExcel = async (req, res) => {
  try {
    const requests = await Request.find()
      .populate('createdBy', 'firstName lastName email')
      .sort({ createdAt: -1 });

    const workbook = new ExcelJS.Workbook();
    const worksheet = workbook.addWorksheet('Requests');

    // Add headers
    worksheet.columns = [
      { header: 'Title', key: 'title', width: 30 },
      { header: 'Description', key: 'description', width: 50 },
      { header: 'Priority', key: 'priority', width: 15 },
      { header: 'Department', key: 'department', width: 20 },
      { header: 'Status', key: 'status', width: 15 },
      { header: 'Created By', key: 'createdBy', width: 30 },
      { header: 'Created At', key: 'createdAt', width: 20 },
      { header: 'Updated At', key: 'updatedAt', width: 20 }
    ];

    // Add rows
    requests.forEach(request => {
      worksheet.addRow({
        title: request.title,
        description: request.description,
        priority: request.priority,
        department: request.department,
        status: request.status,
        createdBy: `${request.createdBy.firstName} ${request.createdBy.lastName}`,
        createdAt: request.createdAt.toLocaleDateString(),
        updatedAt: request.updatedAt.toLocaleDateString()
      });
    });

    // Style the header row
    worksheet.getRow(1).font = { bold: true };
    worksheet.getRow(1).fill = {
      type: 'pattern',
      pattern: 'solid',
      fgColor: { argb: 'FFE0E0E0' }
    };

    // Set response headers
    res.setHeader(
      'Content-Type',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    );
    res.setHeader(
      'Content-Disposition',
      `attachment; filename=requests_${new Date().toISOString().split('T')[0]}.xlsx`
    );

    // Write to response
    await workbook.xlsx.write(res);
    res.end();
  } catch (error) {
    res.status(500).json({
      success: false,
      error: error.message
    });
  }
}; 