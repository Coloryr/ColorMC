//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Avalonia;
//using Avalonia.Controls;
//using Avalonia.LogicalTree;
//using Avalonia.Rendering.Composition;
//using Avalonia.VisualTree;

//namespace ColorMC.Gui.UI.Controls.Skin.Vulkan;

//public class SkinRenderVulkan : Control
//{
//    class VulkanResources : IAsyncDisposable
//    {
//        public VulkanContext Context { get; }
//        public VulkanSwapchain Swapchain { get; }
//        public VulkanContent Content { get; }

//        public VulkanResources(VulkanContext context, VulkanSwapchain swapchain, VulkanContent content)
//        {
//            Context = context;
//            Swapchain = swapchain;
//            Content = content;
//        }
//        public async ValueTask DisposeAsync()
//        {
//            Context.Pool.FreeUsedCommandBuffers();
//            Content.Dispose();
//            await Swapchain.DisposeAsync();
//            Context.Dispose();
//        }
//    }

//    private CompositionSurfaceVisual? _visual;
//    private Compositor? _compositor;
//    private readonly Action _update;
//    private string _info = string.Empty;
//    private bool _updateQueued;
//    private bool _initialized;

//    protected CompositionDrawingSurface? Surface { get; private set; }

//    public SkinRenderVulkan()
//    {
//        _update = UpdateFrame;
//    }

//    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
//    {
//        base.OnAttachedToVisualTree(e);
//        Initialize();
//    }

//    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
//    {
//        if (_initialized)
//            FreeGraphicsResources();
//        _initialized = false;
//        base.OnDetachedFromLogicalTree(e);
//    }

//    async void Initialize()
//    {
//        try
//        {
//            var selfVisual = ElementComposition.GetElementVisual(this)!;
//            _compositor = selfVisual.Compositor;

//            Surface = _compositor.CreateDrawingSurface();
//            _visual = _compositor.CreateSurfaceVisual();
//            _visual.Size = new(Bounds.Width, Bounds.Height);
//            _visual.Surface = Surface;
//            ElementComposition.SetElementChildVisual(this, _visual);
//            var (res, info) = await DoInitialize(_compositor, Surface);
//            _info = info;
//            _initialized = res;
//            QueueNextFrame();
//        }
//        catch (Exception e)
//        {

//        }
//    }

//    void UpdateFrame()
//    {
//        _updateQueued = false;
//        var root = this.GetVisualRoot();
//        if (root == null)
//            return;

//        _visual!.Size = new(Bounds.Width, Bounds.Height);
//        var size = PixelSize.FromSize(Bounds.Size, root.RenderScaling);
//        RenderFrame(size);
//        QueueNextFrame();
//    }

//    void QueueNextFrame()
//    {
//        if (_initialized && !_updateQueued && _compositor != null)
//        {
//            _updateQueued = true;
//            _compositor?.RequestCompositionUpdate(_update);
//        }
//    }

//    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
//    {
//        if (change.Property == BoundsProperty)
//            QueueNextFrame();
//        base.OnPropertyChanged(change);
//    }

//    async Task<(bool success, string info)> DoInitialize(Compositor compositor,
//        CompositionDrawingSurface compositionDrawingSurface)
//    {
//        var interop = await compositor.TryGetCompositionGpuInterop();
//        if (interop == null)
//            return (false, "Compositor doesn't support interop for the current backend");
//        return InitializeGraphicsResources(compositor, compositionDrawingSurface, interop);
//    }

//    protected virtual bool SupportsDisco => false;

//    private VulkanResources? _resources;

//    protected (bool success, string info) InitializeGraphicsResources(Compositor compositor,
//        CompositionDrawingSurface compositionDrawingSurface, ICompositionGpuInterop gpuInterop)
//    {
//        var (context, info) = VulkanContext.TryCreate(gpuInterop);
//        if (context == null)
//            return (false, info);
//        try
//        {
//            var content = new VulkanContent(context);
//            _resources = new VulkanResources(context,
//                new VulkanSwapchain(context, gpuInterop, compositionDrawingSurface), content);
//            return (true, info);
//        }
//        catch (Exception e)
//        {
//            return (false, e.ToString());
//        }
//    }

//    protected void FreeGraphicsResources()
//    {
//        _resources?.DisposeAsync();
//        _resources = null;
//    }

//    protected unsafe void RenderFrame(PixelSize pixelSize)
//    {
//        if (_resources == null)
//            return;
//        using (_resources.Swapchain.BeginDraw(pixelSize, out var image))
//        {

//        }
//    }
//}
