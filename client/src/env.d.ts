/// <reference types="vite/client" />

declare module '*.vue' {
    import type { DefineComponent } from 'vue'
    const component: DefineComponent<{}, {}, any>
    export default component
}

declare module 'html2pdf.js' {
    function html2pdf(): any
    export = html2pdf
}

declare module 'cobe' {
    function createGlobe(canvas: HTMLCanvasElement, options: object): { destroy(): void }
    export = createGlobe
}
