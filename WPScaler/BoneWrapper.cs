using SoulsFormats;

namespace WPScaler
{
    class BoneWrapper
    {
        private FLVER.Bone bone;

        public string Name => bone.Name;

        public float ScaleX
        {
            get => bone.Scale.X;
            set => bone.Scale.X = value;
        }

        public float ScaleY
        {
            get => bone.Scale.Y;
            set => bone.Scale.Y = value;
        }

        public float ScaleZ
        {
            get => bone.Scale.Z;
            set => bone.Scale.Z = value;
        }

        public BoneWrapper(FLVER.Bone bone)
        {
            this.bone = bone;
        }
    }
}
