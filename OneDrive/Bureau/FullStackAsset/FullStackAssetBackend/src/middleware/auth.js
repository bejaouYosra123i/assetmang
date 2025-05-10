const jwt = require('jsonwebtoken');

module.exports = (req, res, next) => {
    try {
        // Récupérer le token du header
        const token = req.headers.authorization.split(' ')[1];
        
        // Vérifier le token
        const decodedToken = jwt.verify(token, process.env.JWT_SECRET);
        
        // Ajouter l'userId à la requête
        req.userId = decodedToken.userId;
        
        next();
    } catch (error) {
        res.status(401).json({ message: 'Non authentifié' });
    }
}; 