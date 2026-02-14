using Godot;
using System;

namespace Firebyte
{
    /// <summary>
    /// Gestionnaire de secousse de caméra simplifié compatible Godot 4.2
    /// </summary>
    public partial class CameraShake : Node
    {
        // Références
        private Camera3D _camera;
        private Random _random = new Random();

        // Paramètres de secousse
        [Export] public float ShakeIntensity { get; set; } = 0.5f;
        [Export] public float ShakeDuration { get; set; } = 0.3f;
        [Export] public float ShakeSpeed { get; set; } = 15.0f;

        // État interne
        private bool _isShaking = false;
        private float _shakeTime = 0.0f;
        private Vector3 _originalPosition;
        private Vector3 _shakeOffset;

        public override void _Ready()
        {
            GD.Print("📷 Initialisation du gestionnaire de secousse de caméra...");
            
            // Trouver la caméra parente
            _camera = GetParent() as Camera3D;
            if (_camera == null)
            {
                // Chercher dans les enfants
                foreach (Node child in GetChildren())
                {
                    if (child is Camera3D cam)
                    {
                        _camera = cam;
                        break;
                    }
                }
            }

            if (_camera != null)
            {
                _originalPosition = _camera.Position;
                GD.Print("✅ Gestionnaire de secousse de caméra initialisé");
            }
            else
            {
                GD.Print("⚠️ Aucune caméra trouvée pour le CameraShake");
            }
        }

        public override void _Process(double delta)
        {
            if (!_isShaking || _camera == null) return;

            var deltaTime = (float)delta;
            _shakeTime += deltaTime;

            if (_shakeTime >= ShakeDuration)
            {
                // Fin de la secousse
                _isShaking = false;
                _shakeTime = 0.0f;
                _camera.Position = _originalPosition;
                return;
            }

            // Calculer la secousse
            var progress = _shakeTime / ShakeDuration;
            var intensity = ShakeIntensity * (1.0f - progress); // Diminue progressivement

            // Mouvement aléatoire
            var randomX = (float)(_random.NextDouble() * 2.0f - 1.0f) * intensity;
            var randomY = (float)(_random.NextDouble() * 2.0f - 1.0f) * intensity;
            var randomZ = (float)(_random.NextDouble() * 2.0f - 1.0f) * intensity;

            _shakeOffset = new Vector3(randomX, randomY, randomZ);
            _camera.Position = _originalPosition + _shakeOffset;
        }

        /// <summary>
        /// Déclenche une secousse de caméra pour le tir
        /// </summary>
        public void ShakeFromShoot()
        {
            GD.Print("📷 Secousse de caméra pour le tir");
            StartShake(0.2f, 0.1f);
        }

        /// <summary>
        /// Déclenche une secousse de caméra pour l'impact
        /// </summary>
        public void ShakeFromImpact()
        {
            GD.Print("📷 Secousse de caméra pour l'impact");
            StartShake(0.4f, 0.2f);
        }

        /// <summary>
        /// Déclenche une secousse de caméra pour les dégâts
        /// </summary>
        public void ShakeFromDamage()
        {
            GD.Print("📷 Secousse de caméra pour les dégâts");
            StartShake(0.6f, 0.3f);
        }

        /// <summary>
        /// Déclenche une secousse de caméra personnalisée
        /// </summary>
        public void ShakeFromImpact(float impactForce)
        {
            GD.Print($"📷 Secousse de caméra personnalisée: {impactForce}");
            var intensity = Mathf.Clamp(impactForce * 0.1f, 0.1f, 1.0f);
            var duration = Mathf.Clamp(impactForce * 0.05f, 0.05f, 0.5f);
            StartShake(intensity, duration);
        }

        /// <summary>
        /// Démarre une secousse de caméra
        /// </summary>
        private void StartShake(float intensity, float duration)
        {
            if (_camera == null) return;

            if (!_isShaking)
            {
                _originalPosition = _camera.Position;
            }

            ShakeIntensity = intensity;
            ShakeDuration = duration;
            _isShaking = true;
            _shakeTime = 0.0f;
        }

        /// <summary>
        /// Arrête la secousse de caméra
        /// </summary>
        public void StopShake()
        {
            _isShaking = false;
            _shakeTime = 0.0f;
            
            if (_camera != null)
            {
                _camera.Position = _originalPosition;
            }
        }

        /// <summary>
        /// Définit la caméra cible
        /// </summary>
        public void SetCamera(Camera3D camera)
        {
            _camera = camera;
            if (_camera != null)
            {
                _originalPosition = _camera.Position;
            }
        }

        /// <summary>
        /// Nettoie les ressources
        /// </summary>
        public override void _ExitTree()
        {
            GD.Print("🧹 Nettoyage du gestionnaire de secousse de caméra...");
            
            _camera = null;
        }
    }
}
