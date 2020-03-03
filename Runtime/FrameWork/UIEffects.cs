using System;
using TweenUtil;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIFrameWork
{
    [Serializable]
    public class UIEffects
    {
        [Serializable]
        public class PositionEffectData
        {
            public float _Time = 0.1f;
            public Vector3 _Offset;
            public EaseType _PositionEffect = EaseType.EaseInOutBack;
            public UIBehaviour _Widget;
        }

        [Serializable]
        public class ScaleEffectData
        {
            public float _Time = 0.1f;
            public Vector3 _Scale = Vector2.one * 1.5f;
            public EaseType _ScaleEffect = EaseType.EaseInOutBack;
            public UIBehaviour _Widget;
        }

        [Serializable]
        public class ColorEffectData
        {
            public float _Time = 0.1f;
            public Color _Color = Color.grey;
            public EaseType _ColorEffect = EaseType.EaseInOutBack;
            public UIBehaviour _Widget;
        }

        [Serializable]
        public class SpriteEffectData
        {
            public Sprite _Sprite;
            public UIBehaviour _Widget;
        }

        [Serializable]
        public class ParticleEffectData
        {
            public ParticleSystem _Particle;
            public float _StartSize;
        }

        public abstract class EffectBase<ModifyType, WidgetDataType>
        {
            public bool _UseEffect;
            public WidgetDataType[] _ApplyTo;

            protected ModifyType[] mOriginalValues;

            public virtual void CacheOriginalValues()
            {
                mOriginalValues = new ModifyType[_ApplyTo.Length];

                for (int i = 0; i < _ApplyTo.Length; ++i)
                    CacheOriginalValue(i);
            }

            public abstract void CacheOriginalValue(int index);

            public abstract void ShowEffect(bool showEffect);
        }

        [Serializable]
        public class PositionEffect : EffectBase<Vector3, PositionEffectData>
        {
            public override void CacheOriginalValue(int index)
            {
                if (_ApplyTo[index]._Widget != null)
                    mOriginalValues[index] = _ApplyTo[index]._Widget.transform.localPosition;
            }

            public override void ShowEffect(bool showEffect)
            {
                for (int i = 0; i < _ApplyTo.Length; ++i)
                {
                    PositionEffectData positionEffectData = _ApplyTo[i];

                    if (positionEffectData._Widget != null)
                    {
                        Vector3 toPosition = showEffect ? mOriginalValues[i] + positionEffectData._Offset : mOriginalValues[i];
                        Tween.MoveLocalTo(positionEffectData._Widget.gameObject, positionEffectData._Widget.transform.localPosition, toPosition, new TweenParam(positionEffectData._Time, positionEffectData._PositionEffect));
                    }
                }
            }
        }

        [Serializable]
        public class ScaleEffect : EffectBase<Vector3, ScaleEffectData>
        {
            public override void CacheOriginalValue(int index)
            {
                if (_ApplyTo[index]._Widget != null)
                    mOriginalValues[index] = _ApplyTo[index]._Widget.transform.localScale;
            }

            public override void ShowEffect(bool showEffect)
            {
                for (int i = 0; i < _ApplyTo.Length; ++i)
                {
                    ScaleEffectData scaleEffectData = _ApplyTo[i];

                    if (scaleEffectData._Widget != null)
                    {
                        Vector3 toScale = showEffect ? Vector3.Scale(mOriginalValues[i], scaleEffectData._Scale) : mOriginalValues[i];
                        Tween.ScaleTo(scaleEffectData._Widget.gameObject, scaleEffectData._Widget.transform.localScale, toScale, new TweenParam(scaleEffectData._Time, scaleEffectData._ScaleEffect));
                    }
                }
            }
        }

        [Serializable]
        public class ColorEffect : EffectBase<Color, ColorEffectData>
        {
            public override void CacheOriginalValue(int index)
            {
                Graphic graphic = _ApplyTo[index]._Widget as Graphic;
                if (graphic != null)
                    mOriginalValues[index] = graphic.color;
            }

            public override void ShowEffect(bool showEffect)
            {
                for (int i = 0; i < _ApplyTo.Length; ++i)
                {
                    ColorEffectData colorEffectData = _ApplyTo[i];

                    if (colorEffectData._Widget != null)
                    {
                        Graphic graphic = colorEffectData._Widget as Graphic;
                        if (graphic != null)
                        {
                            Color toColor = showEffect ? colorEffectData._Color : mOriginalValues[i];
                            Tween.ColorTo(graphic.gameObject, graphic.color, toColor, new TweenParam(colorEffectData._Time, colorEffectData._ColorEffect));
                        }
                    }
                }
            }
        }

        [Serializable]
        public class SpriteEffect : EffectBase<Sprite, SpriteEffectData>
        {
            public override void CacheOriginalValue(int index)
            {
                Image image = _ApplyTo[index]._Widget as Image;
                if (image != null)
                    mOriginalValues[index] = image.sprite;
            }

            public override void ShowEffect(bool showEffect)
            {
                for (int i = 0; i < _ApplyTo.Length; ++i)
                {
                    SpriteEffectData spriteEffectData = _ApplyTo[i];

                    if (spriteEffectData._Widget != null)
                    {
                        Image image = spriteEffectData._Widget as Image;
                        if (image != null)
                        {
                            if (showEffect)
                                image.sprite = spriteEffectData._Sprite;
                            else
                                image.sprite = mOriginalValues[i];
                        }
                    }
                }
            }
        }

        [Serializable]
        public class ParticleEffect
        {
            public ParticleEffectData[] _Particles;
        }

        //public Sound _Clip;
        public PositionEffect _PositionEffect = new PositionEffect();
        public ScaleEffect _ScaleEffect = new ScaleEffect();
        public ColorEffect _ColorEffect = new ColorEffect();
        public SpriteEffect _SpriteEffect = new SpriteEffect();
        public ParticleEffect _ParticleEffect = new ParticleEffect();

        public float _MaxDuration = -1;

        //private SoundChannel mChannel = null;

        public bool pIsOn { get; private set; }

        public void PlaySound(bool on)
        {
            //if (on)
            //{
            //	if (_Clip._AudioClip != null && !string.IsNullOrEmpty(_Clip._Settings._Pool))
            //		mChannel = SoundChannel.Play(_Clip._AudioClip, _Clip._Settings, _Clip._Triggers, false);
            //}
            //else if (mChannel != null)
            //{
            //	// Stop the channel only if it is still playing the sound we played
            //	if (SoundChannel.ClipCompare(mChannel.pClip, _Clip._AudioClip))
            //		mChannel.Stop();
            //	mChannel = null;
            //}
        }

        public void PlayParticle(bool isPlay)
        {
            if (_ParticleEffect == null || _ParticleEffect._Particles == null || _ParticleEffect._Particles.Length == 0)
                return;

            for (int i = 0; i < _ParticleEffect._Particles.Length; i++)
            {
                if (_ParticleEffect._Particles[i]._Particle != null)
                {
                    if (isPlay)
                    {
                        if (_ParticleEffect._Particles[i]._StartSize != 0)
                        {
                            ParticleSystem.MainModule particleMain = _ParticleEffect._Particles[i]._Particle.main;
                            particleMain.startSize = _ParticleEffect._Particles[i]._StartSize;
                        }

                        _ParticleEffect._Particles[i]._Particle.Play();
                    }
                    else
                        _ParticleEffect._Particles[i]._Particle.Stop();
                }
            }
        }
    }
}