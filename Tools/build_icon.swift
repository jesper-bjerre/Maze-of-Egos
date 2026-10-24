// Original vector icon. Run from the repository root with `swift Tools/build_icon.swift`.
import AppKit
import ImageIO
import UniformTypeIdentifiers
let size = 1024
let context = CGContext(data: nil, width: size, height: size, bitsPerComponent: 8, bytesPerRow: size * 4, space: CGColorSpaceCreateDeviceRGB(), bitmapInfo: CGImageAlphaInfo.noneSkipLast.rawValue)!
context.translateBy(x: 0, y: CGFloat(size)); context.scaleBy(x: 1, y: -1)
let background = NSColor(srgbRed: 0.055, green: 0.10, blue: 0.12, alpha: 1)
let gold = NSColor(srgbRed: 0.94, green: 0.72, blue: 0.35, alpha: 1)
let pink = NSColor(srgbRed: 0.96, green: 0.20, blue: 0.32, alpha: 1)
context.setFillColor(background.cgColor); context.fill(CGRect(x: 0, y: 0, width: size, height: size))
let lines: [[CGPoint]] = [
    [.init(x:160,y:405),.init(x:160,y:160),.init(x:590,y:160)],
    [.init(x:730,y:160),.init(x:864,y:160),.init(x:864,y:610)],
    [.init(x:864,y:750),.init(x:864,y:864),.init(x:430,y:864)],
    [.init(x:290,y:864),.init(x:160,y:864),.init(x:160,y:550)],
    [.init(x:302,y:340),.init(x:302,y:302),.init(x:710,y:302)],
    [.init(x:722,y:440),.init(x:722,y:722),.init(x:610,y:722)],
    [.init(x:470,y:722),.init(x:302,y:722),.init(x:302,y:610)]
]
context.setStrokeColor(gold.cgColor); context.setLineWidth(52); context.setLineCap(.round); context.setLineJoin(.round)
for line in lines { context.beginPath(); context.move(to:line[0]); for point in line.dropFirst() {context.addLine(to:point)}; context.strokePath() }
context.setFillColor(pink.cgColor)
context.move(to: .init(x:512,y:650))
context.addCurve(to:.init(x:388,y:442),control1:.init(x:482,y:618),control2:.init(x:342,y:510))
context.addCurve(to:.init(x:512,y:454),control1:.init(x:418,y:396),control2:.init(x:484,y:414))
context.addCurve(to:.init(x:636,y:442),control1:.init(x:540,y:414),control2:.init(x:606,y:396))
context.addCurve(to:.init(x:512,y:650),control1:.init(x:682,y:510),control2:.init(x:542,y:618))
context.closePath(); context.fillPath()
let destination = CGImageDestinationCreateWithURL(URL(fileURLWithPath: "Assets/Resources/Brand/AppIcon.png") as CFURL, UTType.png.identifier as CFString, 1, nil)!
CGImageDestinationAddImage(destination, context.makeImage()!, nil)
precondition(CGImageDestinationFinalize(destination))
let poly = lines.map { line in "<polyline points=\"" + line.map { "\(Int($0.x)),\(Int($0.y))" }.joined(separator:" ") + "\"/>" }.joined(separator:"\n")
let svg = """
<svg xmlns="http://www.w3.org/2000/svg" width="1024" height="1024" viewBox="0 0 1024 1024">
<rect width="1024" height="1024" fill="#0e1a1f"/>
<g fill="none" stroke="#f0b859" stroke-width="52" stroke-linecap="round" stroke-linejoin="round">
\(poly)
</g>
<path fill="#f53352" d="M512 650 C482 618 342 510 388 442 C418 396 484 414 512 454 C540 414 606 396 636 442 C682 510 542 618 512 650Z"/>
</svg>
"""
try svg.write(toFile:"art/icon/app-icon.svg",atomically:true,encoding:.utf8)
print("Created original 1024px RGB icon and editable SVG.")
