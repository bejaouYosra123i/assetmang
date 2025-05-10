const User = require('../models/User');
const bcrypt = require('bcryptjs');
const { RolesEnum } = require('../utils/roles');

// Vérifier le mot de passe et supprimer l'utilisateur
exports.verifyAndDelete = async (req, res) => {
    try {
        const { userId, password } = req.body;
        const currentUserId = req.userId; // Récupéré du middleware d'authentification

        // Récupérer l'utilisateur actuel (celui qui fait la suppression)
        const currentUser = await User.findById(currentUserId);
        if (!currentUser) {
            return res.status(404).json({ message: 'Utilisateur actuel non trouvé' });
        }

        // Récupérer l'utilisateur à supprimer
        const userToDelete = await User.findById(userId);
        if (!userToDelete) {
            return res.status(404).json({ message: 'Utilisateur à supprimer non trouvé' });
        }

        // Vérifier les autorisations
        const isAuthorized = isAuthorizedForDelete(currentUser.roles[0], userToDelete.roles[0]);
        if (!isAuthorized) {
            return res.status(403).json({ message: 'Non autorisé à supprimer cet utilisateur' });
        }

        // Vérifier le mot de passe de l'utilisateur actuel (celui qui fait la suppression)
        const isPasswordValid = await bcrypt.compare(password, currentUser.password);
        if (!isPasswordValid) {
            return res.status(401).json({ message: 'Votre mot de passe est incorrect' });
        }

        // Empêcher un utilisateur de se supprimer lui-même
        if (currentUserId === userId) {
            return res.status(400).json({ message: 'Vous ne pouvez pas supprimer votre propre compte' });
        }

        // Supprimer l'utilisateur
        await User.findByIdAndDelete(userId);

        res.status(200).json({ message: 'Utilisateur supprimé avec succès' });
    } catch (error) {
        console.error('Erreur lors de la suppression:', error);
        res.status(500).json({ message: 'Erreur lors de la suppression de l\'utilisateur' });
    }
};

// Fonction utilitaire pour vérifier les autorisations
const isAuthorizedForDelete = (currentUserRole, targetUserRole) => {
    if (currentUserRole === RolesEnum.ADMIN) return true;
    if (currentUserRole === RolesEnum.MANAGER && targetUserRole === RolesEnum.USER) return true;
    return false;
}; 