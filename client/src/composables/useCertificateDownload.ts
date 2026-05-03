import { ref } from 'vue'
import type { CertificateItem } from '@/utils/models'

/**
 * Shared composable — renders the certificate design into an off-screen
 * container and triggers an html2pdf download. Works from any page without
 * requiring navigation to CertificatePage.
 */
export function useCertificateDownload() {
  const downloading = ref<number | null>(null)   // holds cert.id while downloading

  // ── helpers ────────────────────────────────────────────────────────────────
  function scoreLabel(p: number) {
    if (p >= 90) return 'with Distinction'
    if (p >= 75) return 'with Merit'
    return 'with Completion'
  }

  function fmt(iso: string) {
    return new Date(iso).toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' })
  }

  // ── certificate HTML builder ───────────────────────────────────────────────
  function buildCertificateHTML(cert: CertificateItem): string {
    const company  = cert.companyName || 'EVALO.me'
    const score    = cert.percentage.toFixed(1)
    const label    = scoreLabel(cert.percentage)
    const issued   = fmt(cert.issuedAt)

    return /* html */ `
      <style data-cert-fonts>
        @import url('https://fonts.googleapis.com/css2?family=Cormorant+Garamond:ital,wght@0,400;0,600;1,400&family=Great+Vibes&display=swap');
      </style>
      <div style="
        position:relative;background:#fff;width:1120px;height:793px;
        border:14px solid #1a2e4a;font-family:'Cormorant Garamond',Georgia,serif;
        overflow:hidden;box-sizing:border-box;
      ">
        <!-- inner border -->
        <div style="position:absolute;inset:18px;border:1.5px solid rgba(26,46,74,0.25);pointer-events:none;"></div>

        <!-- dot pattern -->
        <div style="position:absolute;inset:0;opacity:0.03;
          background-image:radial-gradient(circle,#1a2e4a 1px,transparent 1px);
          background-size:28px 28px;"></div>

        <!-- corners -->
        <svg style="position:absolute;top:3px;left:3px;width:80px;height:80px;" viewBox="0 0 80 80" fill="none">
          <path d="M8 8 L40 8 M8 8 L8 40" stroke="#1a2e4a" stroke-width="2"/>
          <path d="M14 14 L36 14 M14 14 L14 36" stroke="#1a2e4a" stroke-width="1" stroke-dasharray="3 3"/>
          <circle cx="8" cy="8" r="3" fill="#1a2e4a"/>
          <path d="M20 8 Q14 14 8 20" stroke="#b8860b" stroke-width="1.5" fill="none"/>
        </svg>
        <svg style="position:absolute;top:3px;right:3px;width:80px;height:80px;" viewBox="0 0 80 80" fill="none">
          <path d="M72 8 L40 8 M72 8 L72 40" stroke="#1a2e4a" stroke-width="2"/>
          <path d="M66 14 L44 14 M66 14 L66 36" stroke="#1a2e4a" stroke-width="1" stroke-dasharray="3 3"/>
          <circle cx="72" cy="8" r="3" fill="#1a2e4a"/>
          <path d="M60 8 Q66 14 72 20" stroke="#b8860b" stroke-width="1.5" fill="none"/>
        </svg>
        <svg style="position:absolute;bottom:3px;left:3px;width:80px;height:80px;" viewBox="0 0 80 80" fill="none">
          <path d="M8 72 L40 72 M8 72 L8 40" stroke="#1a2e4a" stroke-width="2"/>
          <path d="M14 66 L36 66 M14 66 L14 44" stroke="#1a2e4a" stroke-width="1" stroke-dasharray="3 3"/>
          <circle cx="8" cy="72" r="3" fill="#1a2e4a"/>
          <path d="M20 72 Q14 66 8 60" stroke="#b8860b" stroke-width="1.5" fill="none"/>
        </svg>
        <svg style="position:absolute;bottom:3px;right:3px;width:80px;height:80px;" viewBox="0 0 80 80" fill="none">
          <path d="M72 72 L40 72 M72 72 L72 40" stroke="#1a2e4a" stroke-width="2"/>
          <path d="M66 66 L44 66 M66 66 L66 44" stroke="#1a2e4a" stroke-width="1" stroke-dasharray="3 3"/>
          <circle cx="72" cy="72" r="3" fill="#1a2e4a"/>
          <path d="M60 72 Q66 66 72 60" stroke="#b8860b" stroke-width="1.5" fill="none"/>
        </svg>

        <!-- left accent bar -->
        <div style="position:absolute;left:40px;top:40px;bottom:40px;width:3px;
          background:linear-gradient(to bottom,transparent,#1a2e4a 20%,#b8860b 50%,#1a2e4a 80%,transparent);"></div>

        <!-- main content -->
        <div style="display:flex;flex-direction:column;align-items:center;justify-content:center;
          height:100%;gap:10px;padding:0 112px;color:#1a2e4a;">

          <p style="font-size:11px;letter-spacing:0.5em;text-transform:uppercase;
            font-weight:600;color:rgba(26,46,74,0.5);line-height:1;margin:0;">
            ${company}
          </p>

          <!-- decorative line -->
          <div style="display:flex;align-items:center;gap:12px;width:256px;margin:0;">
            <div style="flex:1;height:1px;background:linear-gradient(to right,transparent,#b8860b);"></div>
            <svg width="12" height="12" viewBox="0 0 12 12"><polygon points="6,0 12,6 6,12 0,6" fill="#b8860b"/></svg>
            <div style="flex:1;height:1px;background:linear-gradient(to left,transparent,#b8860b);"></div>
          </div>

          <h1 style="font-size:28px;font-weight:600;letter-spacing:0.22em;text-transform:uppercase;
            color:#1a2e4a;line-height:1.1;margin:0;">
            Certificate of Completion
          </h1>

          <p style="font-size:13px;font-style:italic;color:rgba(26,46,74,0.6);
            line-height:1;letter-spacing:0.05em;margin:0;">
            This is to certify that
          </p>

          <h2 style="font-family:'Great Vibes',cursive;font-size:58px;color:#1a2e4a;
            line-height:1;margin:-4px 0 0 0;">
            ${cert.candidateName}
          </h2>

          <p style="font-size:13px;font-style:italic;color:rgba(26,46,74,0.6);
            line-height:1;letter-spacing:0.05em;margin:0;">
            has successfully completed
          </p>

          <h3 style="font-size:20px;font-weight:600;color:#1a2e4a;text-align:center;
            max-width:560px;line-height:1.3;letter-spacing:0.05em;margin:0;">
            ${cert.campaignName}
          </h3>

          <div style="display:flex;align-items:center;gap:8px;font-size:13px;
            color:rgba(26,46,74,0.7);margin:0;">
            <span>${label} · Score:</span>
            <span style="font-weight:700;color:#1a2e4a;font-size:15px;">${score}%</span>
            <span>· Issued on</span>
            <span style="font-weight:600;">${issued}</span>
          </div>

          <!-- decorative line -->
          <div style="display:flex;align-items:center;gap:12px;width:256px;margin:4px 0 0 0;">
            <div style="flex:1;height:1px;background:linear-gradient(to right,transparent,#b8860b);"></div>
            <svg width="12" height="12" viewBox="0 0 12 12"><polygon points="6,0 12,6 6,12 0,6" fill="#b8860b"/></svg>
            <div style="flex:1;height:1px;background:linear-gradient(to left,transparent,#b8860b);"></div>
          </div>

          <!-- signatures -->
          <div style="display:flex;align-items:flex-end;justify-content:space-between;
            width:100%;max-width:512px;margin:4px 0 0 0;">
            <div style="display:flex;flex-direction:column;align-items:center;gap:4px;">
              <div style="height:1px;width:144px;background:#1a2e4a;"></div>
              <p style="font-size:10px;letter-spacing:0.2em;text-transform:uppercase;
                color:rgba(26,46,74,0.5);margin:0;">Team Leader</p>
            </div>
            <div style="display:flex;flex-direction:column;align-items:center;justify-content:center;
              width:68px;height:68px;border-radius:50%;border:3px solid #1a2e4a;position:relative;">
              <div style="position:absolute;inset:5px;border-radius:50%;border:1px solid rgba(26,46,74,0.3);"></div>
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="#1a2e4a" stroke-width="2">
                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/>
                <polyline points="22 4 12 14.01 9 11.01"/>
              </svg>
              <p style="font-size:7px;font-weight:700;text-transform:uppercase;letter-spacing:0.1em;
                color:#1a2e4a;margin:2px 0 0 0;text-align:center;line-height:1.1;">Certified</p>
            </div>
            <div style="display:flex;flex-direction:column;align-items:center;gap:4px;">
              <div style="height:1px;width:144px;background:#1a2e4a;"></div>
              <p style="font-size:10px;letter-spacing:0.2em;text-transform:uppercase;
                color:rgba(26,46,74,0.5);margin:0;">Manager</p>
            </div>
          </div>
        </div>

        <!-- footer cert code -->
        <p style="position:absolute;bottom:10px;left:50%;transform:translateX(-50%);
          font-size:9px;letter-spacing:0.35em;text-transform:uppercase;
          color:rgba(26,46,74,0.3);white-space:nowrap;margin:0;">
          Certificate ID: ${cert.certificateCode}
        </p>
      </div>
    `
  }

  // ── main download function ─────────────────────────────────────────────────
  async function downloadCertificate(cert: CertificateItem) {
    if (downloading.value !== null) return
    downloading.value = cert.id

    // Mount off-screen so Google Fonts have a chance to be applied
    const wrapper = document.createElement('div')
    wrapper.style.cssText = 'position:fixed;top:-9999px;left:-9999px;width:1120px;height:793px;background:#fff;'
    wrapper.innerHTML = buildCertificateHTML(cert)
    document.body.appendChild(wrapper)

    // Collect every page stylesheet/style that carries oklch() tokens.
    // We temporarily detach them before html2canvas clones the document so it
    // never sees oklch values.  They are restored in the finally block.
    type Stashed = { parent: ParentNode; node: Element; before: ChildNode | null }
    const stashed: Stashed[] = []
    document.querySelectorAll<Element>(
      'link[rel="stylesheet"], style:not([data-cert-fonts])',
    ).forEach(node => {
      if (node.parentNode) {
        stashed.push({ parent: node.parentNode, node, before: node.nextSibling })
        node.remove()
      }
    })

    try {
      const html2pdf = (await import('html2pdf.js')).default

      // Small delay so Google Fonts load inside the off-screen wrapper
      await new Promise(r => setTimeout(r, 600))

      const el = wrapper.querySelector('div[style]') as HTMLElement

      await html2pdf().set({
        margin:      0,
        filename:    `certificate-${cert.certificateCode}.pdf`,
        image:       { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2, useCORS: true, logging: false, backgroundColor: '#ffffff' },
        jsPDF:       { unit: 'px', format: [1120, 793], orientation: 'landscape' },
      }).from(el).save()
    } catch (e) {
      console.error('Certificate download failed', e)
    } finally {
      // Restore page stylesheets in original DOM order
      stashed.forEach(({ parent, node, before }) => parent.insertBefore(node, before))
      document.body.removeChild(wrapper)
      downloading.value = null
    }
  }

  return { downloading, downloadCertificate }
}
