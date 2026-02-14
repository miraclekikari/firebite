using Godot;

namespace Firebyte
{
    /// <summary>
    /// Système de Camera Shake pour le feedback visuel
    /// </summary>
    public partial class CameraShake : Node
    {
        // Référence à la caméra
        private Camera3D _camera;
        
        // Paramètres du shake
        [Export] public float ShakeIntensity { get; set; } = 0.5f;
        [Export] public float ShakeDuration { get; set; } = 0.3f;
        [Export] public float ShakeDecay { get; set; } = 0.8f;
        
        // État interne
        private float _currentIntensity = 0.0f;
        private float _currentDuration = 0.0f;
        private Vector3 _originalPosition;
        private bool _isShaking = false;

        public override void _Ready()
        {
            GD.Print("📸 CameraShake initialisé");
        }

        /// <summary>
        /// Configure la caméra à secouer
        /// </summary>
        public void SetupCamera(Camera3D camera)
        {
            _camera = camera;
            _originalPosition = camera.Position;
        }

        /// <summary>
        /// Démarre un shake de caméra
        /// </summary>
        public void Shake(float intensity = -1, float duration = -1)
        {
            if (_camera == null) return;
            
            // Utiliser les valeurs par défaut si non spécifiées
            var shakeIntensity = intensity < 0 ? ShakeIntensity : intensity;
            var shakeDuration = duration < 0 ? ShakeDuration : duration;
            
            // Combiner avec le shake actuel
            _currentIntensity = Mathf.Max(_currentIntensity, shakeIntensity);
            _currentDuration = Mathf.Max(_currentDuration, shakeDuration);
            
            if (!_isShaking)
            {
                _isShaking = true;
                _originalPosition = _camera.Position;
            }
            
            GD.Print($"📸 Camera shake: intensité={shakeIntensity:F2}, durée={shakeDuration:F2}s");
        }

        /// <summary>
        /// Shake spécifique pour le tir
        /// </summary>
        public void ShakeFromShoot()
        {
            Shake(0.3f, 0.15f);
        }

        /// <summary>
        /// Shake spécifique pour l'impact
        /// </summary>
        public void ShakeFromImpact(float impactForce)
        {
            var intensity = Mathf.Clamp(impactForce * 0.1f, 0.2f, 1.5f);
            var duration = Mathf.Clamp(impactForce * 0.05f, 0.1f, 0.5f);
            Shake(intensity, duration);
        }

        /// <summary>
        /// Shake spécifique pour l'explosion
        /// </summary>
        public void ShakeFromExplosion()
        {
            Shake(1.2f, 0.8f);
        }

        /// <summary>
        /// Shake spécifique pour les dégâts
        /// </summary>
        public void ShakeFromDamage()
        {
            Shake(0.6f, 0.25f);
        }

        public override void _Process(double delta)
        {
            if (!_isShaking || _camera == null) return;

            var deltaTime = (float)delta;

            // Mettre à jour la durée
            _currentDuration -= deltaTime;
            
            if (_currentDuration <= 0)
            {
                // Fin du shake
                _camera.Position = _originalPosition;
                _isShaking = false;
                _currentIntensity = 0;
                return;
            }

            // Calculer le shake actuel
            var decayedIntensity = _currentIntensity * Mathf.Pow(ShakeDecay, (ShakeDuration - _currentDuration));
            
            // Génération du mouvement de shake
            var shakeOffset = new Vector3();
            shakeOffset.X = (float)GD.RandRange(-1.0, 1.0) * decayedIntensity;
            shakeOffset.Y = (float)GD.RandRange(-1.0, 1.0) * decayedIntensity;
            shakeOffset.Z = (float)GD.RandRange(-1.0, 1.0) * decayedIntensity * 0.5f; // Moins de Z
            
            // Appliquer le shake
            _camera.Position = _originalPosition + shakeOffset;
        }

        /// <summary>
        /// Arrête immédiatement le shake
        /// </summary>
        public void StopShake()
        {
            if (_camera != null)
            {
                _camera.Position = _originalPosition;
            }
            
            _isShaking = false;
            _currentIntensity = 0;
            _currentDuration = 0;
        }

        /// <summary>
        /// Vérifie si un shake est en cours
        /// </summary>
        public bool IsShaking()
        {
            return _isShaking;
        }

        /// <summary>
        /// Obtient l'intensité actuelle du shake
        /// </summary>
        public float GetCurrentIntensity()
        {
            return _currentIntensity;
        }
    }
}
