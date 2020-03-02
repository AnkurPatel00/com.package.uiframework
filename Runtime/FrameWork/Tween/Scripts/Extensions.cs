using UnityEngine;
using UnityEngine.UI;

namespace TweenUtil
{
	public static class Extensions
	{
        public static void SetPosition(this Transform transform, Vector3 position)
        {
            transform.position = position;
        }

        public static void SetPosition(this Transform transform, Vector2 position)
		{
            Vector3 temp = position;
            temp.z = transform.position.z;
            transform.position = temp;	
		}

		public static void SetLocalPosition(this Transform transform, Vector3 position)
		{
              transform.localPosition = position;	
		}
		
		public static void SetLocalPosition(this Transform transform, Vector2 position)
		{
            Vector3 temp = position;
            temp.z = transform.localPosition.z;
            transform.localPosition = temp;	
		}
	
		public static void SetLocalScale(this Transform transform, Vector3 scale)
		{
			transform.localScale = scale;
		}
	
		public static void SetLocalScale(this Transform transform, Vector2 scale)
		{
            Vector3 temp = scale;
            temp.z = transform.localScale.z;
			transform.localScale = temp;		
		}

		public static void SetRotation(this Transform transform, Vector3 angle)
		{
			transform.rotation = Quaternion.Euler(angle);
		}

        public static void SetLocalRotation(this Transform transform, Vector3 angle)
        {
            transform.localRotation = Quaternion.Euler(angle);
        }

        public static void SetColor(this Renderer renderer, Vector4 color)
        {
            renderer.material.color = color;
        }

        public static void SetColor(this TextMesh textMesh, Vector4 color)
        {
            textMesh.color = color;
        }

		public static void SetColor(this Graphic uiGraphic, Vector4 color)
		{
			uiGraphic.color = color;
		}

		public static void SetAlpha(this Renderer renderer, float val)
		{
			for (int i = 0; i< renderer.materials.Length; ++i)
			{
				Color temp = renderer.material.color;
				temp.a = val;
				renderer.material.color = temp;
			}
		}

        public static void SetText(this TextMesh textMesh, string text)
        {
            textMesh.text = text;
        }

		public static void SetText(this Text uiText, string text)
		{
			uiText.text = text;
		}
    }
}
