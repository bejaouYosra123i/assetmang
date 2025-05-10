const express = require('express');
const router = express.Router();
const userController = require('../controllers/userController');
const auth = require('../middleware/auth');

// Route pour vérifier le mot de passe et supprimer l'utilisateur
router.post('/verify-and-delete', auth, userController.verifyAndDelete);

module.exports = router; 